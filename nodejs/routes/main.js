// メイン（現時点では全部を詰め込んである)
var async = require("async"); 
var path = require('path');
var Busboy = require('busboy');
var os = require('os');
var fs = require('fs');
var im = require('imagemagick');

var network = require("./network");
var filegqs = require("./filegqs");
var dungeon = require('./dungeon');
//var devnull = require('dev-null');
//var html_encoder = require("node-html-encoder").Encoder();

// htmlエンコード(使わない。というか使えない. FAQみたらそんなの必要ないでしょって。)
function simpleHtmlEncode( str ) {

    str = str.replace("\r", "").replace("\n", "<BR>").replace(" ", "&nbsp;").replace("<", "&lt;").replace(">", "&gt;");
    return str;
}

// userinfoを取得(キャッシュがあれば使う)
function getUserInfoCache( client, req, callback ) {

    if ( !req.session.userinfo )
        getUserInfo( client, req, callback );
    else
        callback( null, req.session.userinfo );
}

// userinfoを取得してセットする
function getUserInfo( client, req, callback) {

    async.waterfall([
        function(callback) {
            network.getAllUserInfo(client, callback);
        },
        function(userinfo, callback) {
            req.session.userinfo = userinfo;
            callback();
        }
    ], function(err) {
        callback(err, req.session.userinfo );
    });
}

// トップページに移動する
function gotoTopPage(res) {
    res.render('index', {error_messsage: "ログインしてください"});
}

// ログイン処理
exports.login = function(req, res){

    var email = req.body.mailaddress;
    var password = req.body.password;

    network.connectGodaiQuestServer(email, password, function(err, user_id, client) {
        if ( err ) {
            // すでに同じe-mailの接続があるときは切る
            for(var it in global.connect_gqs) {
                var client = global.connect_gqs[it];
                if ( client.email == email ) {
                    network.closeGodaiQuestServer(client);
                    break;
                }
            }
            res.render('index', { error_message: err, mailaddress:email });
        }
        else {
            // リダイレクトしてログイン処理終了
            req.session.user_id = user_id;
            req.session.client_number = client.number;
            res.redirect('user_list');
        }
    });
//    res.redirect('list');
};

// ログインしているかをチェックする
function checkLogin( req, res ) {

    // 接続チェック
    if( !req.session.user_id ) {
        res.render('index', {error_messsage: "ログインしてください"});
        return null;
    }
    // クライアントを得ることができるかをチェックする
    var client = network.getClient( req.session.client_number );
    if ( !client ) {
        res.render('index', {error_messsage: "ログインしてください"});
        return null;
    }
    return client;
}

// ユーザの未読数一覧
exports.user_list = function(req, res){

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    
    var userinfo;
    var mapUserItem = {};

    async.waterfall([
        function(callback) {
            // ユーザ情報を得る
            getUserInfo( client, req, callback);
        },
        function(_userinfo, callback) {
            userinfo = _userinfo;
            // ダンジョンごとに未読情報を得る
            async.forEach( userinfo.uesr_dic, function(dic, callback) {
                var dungeon_id = dic.auser.user_id;
                network.getUnpickedupItemInfo( client, user_id, dungeon_id, function(err, listItemId) {
                    if ( err ) callback(err);
                    else {
                        mapUserItem[dungeon_id] = listItemId;
                        callback( err );
                    }
                });
            }, function(err) {
                callback(err);
            });
        }
    ], function(err) {
        if ( userinfo ) {
            req.session.userinfo = userinfo; // 保存しておく
        }
        res.render('user_list', {error_message:err, userinfo:userinfo, mapUserItem:mapUserItem} );
    });
}

// ユーザIDに対応する情報を得る
function getAUser( req, user_id, callback) {

    var userinfo = req.session.userinfo.uesr_dic;
    for( var it in userinfo)  {
        var auser = userinfo[it].auser;
        if ( auser.user_id == user_id ) {
            callback( null, auser );
            return;
        }
    }
    callback( "not found a user" );
}

// 一覧に表示する記事の数
var LIST_COUNT = 10;

// 未読の情報一覧表示
exports.info_list = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    var view_id = req.query.view_id;
    var index = req.query.index;
    
    if ( !view_id )
        view_id = user_id;

    // ページめくりの表示
    var flag_before = false, flag_next = false;

    var cnt = 0;
    var userinfo;
    var iteminfoall, iteminfo = {};
    async.waterfall( [
        function(callback) {
            getUserInfoCache( client, req, callback );
        },
        function(_userinfo, callback) {
            userinfo = _userinfo;
            network.getItemInfoByUserId(client, view_id, callback);
        }, 
        function(_iteminfo, callback) {
            iteminfoall = _iteminfo;
            var dungeon_id = view_id;
            network.getUnpickedupItemInfo( client, user_id, dungeon_id, callback );
        },
        function(_listItemId, callback) {
            if ( index === undefined ) {
                index = 0;
            }
            for(var it in _listItemId ) {
                var itemid = _listItemId[it];
                for(var it2 in iteminfoall.aitem_dic ) {
                    var dic = iteminfoall.aitem_dic[it2];
                    if ( dic.aitem.item_id == itemid ) {
//                        dic.aitem.header_string = simpleHtmlEncode( dic.aitem.header_string);
                        if ( cnt < index ) {
                            flag_before = true;
                        }
                        else if ( cnt >= index && cnt < index+LIST_COUNT) {
                            iteminfo[itemid] = dic.aitem; // AItemの情報が入る
                        }
                        else if ( cnt >= index+LIST_COUNT ) {
                            flag_next = true;
                        }
                            
                        ++cnt;
                        break;
                    }
                }
            }
            callback( null );
        },
        function( callback ) {
            getAUser( req, view_id, callback );
        }
    ], function(err, auser) {
        if ( !auser ) auser = {}
        res.render('info_list', {error_message:err, name:auser.user_name, iteminfo:iteminfo, view_id:view_id, index:index, before:flag_before, next:flag_next});
    });
}

// 情報一覧表示
exports.info_list_all = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var userinfo;
    var user_id = req.session.user_id;
    var view_id = req.query.view_id;
    if ( !view_id ) {
        gotoTopPage(res);
        return;
    }
    var index = req.query.index;

    // ページめくりの表示
    var flag_before = false, flag_next = false;

    var iteminfo = {};
    var cnt = 0;
    async.waterfall( [
        function(callback) {
            getUserInfoCache( client, req, callback );
        },
        function(_userinfo, callback) {
            userinfo = _userinfo;
            network.getItemInfoByUserId(client, view_id, callback);
        }, 
        function(_iteminfo, callback) {
            if ( index === undefined ) {
                index = Math.max(0, _iteminfo.aitem_dic.length - LIST_COUNT);
            }
            for(var it2 in _iteminfo.aitem_dic ) {
                var dic = _iteminfo.aitem_dic[it2];
                var itemid = dic.aitem.item_id;
                if ( cnt < index ) {
                    flag_before = true;
                }
                else if ( cnt >= index && cnt < index+LIST_COUNT ) {
                    iteminfo[itemid] = dic.aitem; // AItemの情報が入る
                }
                else if ( cnt >= index + LIST_COUNT ) {
                    flag_next = true;
                }
                ++cnt;
            }
            callback( null );
        },
        function( callback ) {
            getAUser( req, view_id, callback );
        }
    ], function(err, auser) {
        res.render('info_list_all', {allinfo:1, error_message:err, view_id:view_id, name:auser.user_name, iteminfo:iteminfo, index:index, before:flag_before, next:flag_next, pagesize:LIST_COUNT});
    });
}

// 情報表示
exports.info = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var userinfo;
    var user_id = req.session.user_id;
    var view_id = req.query.view_id;
    var info_id = req.query.info_id;
    if ( !view_id || !info_id ) {
        gotoTopPage(res);
        return;
    }

    var iteminfo = {};
    var download_folder;
    var listFiles;
    
    async.waterfall( [
        function(callback) {
            getUserInfoCache( client, req, callback );
        },
        function(_userinfo, callback ) {
            userinfo = _userinfo;
            network.getItemInfoByUserId(client, view_id, callback);
        }, 
        function(_iteminfo, callback) {
            for(var it2 in _iteminfo.aitem_dic ) {
                var dic = _iteminfo.aitem_dic[it2];
                var itemid = dic.aitem.item_id;
                iteminfo[itemid] = dic.aitem; // AItemの情報が入る
                // 一個しか読まないけれども
            }
            download_folder = path.join( global.DOWNLOAD_FOLDER, ""+itemid );
            // アイテム情報を得る
            network.getAItem(client, info_id, callback);
        },
        function(_listFiles, callback) {
            listFiles = _listFiles;
            // 読んだことにする
            network.readMarkArticle( client, view_id, info_id, callback );
        },
        function(callback) {
            // 記事の内容を読む
            network.getArticleString(client, info_id, callback );
        }
    ], function(err, article_content) {
        res.render('info', {error_message:err, user_id:user_id, iteminfo:iteminfo, info_id:info_id, view_id:view_id, listFiles:listFiles, article_content: article_content, pagesize:LIST_COUNT});
    });

}

// 投稿の書き込み
exports.info_post = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var userinfo;
    var user_id = req.session.user_id;
    var view_id = req.query.view_id;
    var info_id = req.query.info_id;
    var contents = req.body.inputtext;
    if ( !view_id || !info_id ) {
        gotoTopPage(res);
        return;
    }

    if ( !contents || contents.length == 0 ) {
        res.redirect("info?view_id="+view_id+"&info_id="+info_id);
        return;
    }

    async.waterfall([
        function(callback) {
            network.setItemArticle( client, info_id, 0, user_id, contents, callback );
        }
    ], function(err) {
        res.redirect("info?view_id="+view_id+"&info_id="+info_id);
    });
}

// 最後に書き込んだ記事を削除する
exports.info_del_article = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var userinfo;
    var user_id = req.session.user_id;
    var view_id = req.query.view_id;
    var info_id = req.query.info_id;
    if ( !view_id || !info_id ) {
        gotoTopPage(res);
        return;
    }

    async.waterfall([
        function(callback) {
            network.deleteLastItemArticle( client, info_id, callback );
        }
    ], function(err) {
        if (err) 
            console.log(err);
        res.redirect("info?view_id="+view_id+"&info_id="+info_id);
    });
}


// 記事の書き込み
exports.write_info = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    var dungeon_id = user_id;
    var level = 0;
    
    var dungeon_info;
    var island_info, island_ground_info;
    var rest_item_cnt;
    var object_attr_info, moto_object_attr_info;
    var block_images_info, tile_info, mapObjIdToImageId;
    async.waterfall([
        // 0. 情報取得フェーズ
        function(callback) {
            // オブジェクトの情報取得
            network.getObjectAttrInfo( client, callback );
        },
        function(_object_attr_info, _moto_object_attr_info, callback) {
            object_attr_info = _object_attr_info;
            moto_object_attr_info = _moto_object_attr_info;
            // イメージ情報
            network.getDungeonImageBlock(client, callback );
        },
        function(_block_images_info, callback) {
            block_images_info = _block_images_info;

            // タイル情報取得
            network.getTileList( client, callback );
        },
        function(_tile_info, callback ){
            tile_info = _tile_info;
            mapObjIdToImageId = dungeon.makeMapObjIdToImageIdFromTileInfo(tile_info);
            callback();
        },
        function(callback) {
            // 1. ダンジョン1階の情報を得る
            network.getDungeon( client, dungeon_id, level, callback );
        },
        function(_dungeon_info, callback) {
            dungeon_info = _dungeon_info;
            callback();
        },
        function(callback) {
            // 2. ダンジョンの空きスペースをチェックする
            rest_item_cnt = dungeon.getDungeonSpaceCnt( dungeon_info, object_attr_info );
            if (rest_item_cnt-1/*階段分*/ == 0 ) {
                callback("アイテムを置くためのスペースがありません。ダンジョンを広げてください")
            }
            else {
                callback();
            }
        }
    ], function(err) {
        res.render('write_info', {error_message:err, rest_item_cnt:rest_item_cnt, content:""});
    });
}

// 記事の書き込み
exports.write_info_post = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    var dungeon_id = user_id;
    var level = 0;

    var text = req.body.inputtext;
    
    var dungeon_info;
    var island_info, island_ground_info;
    var rest_item_cnt;
    var object_attr_info, moto_object_attr_info;
    var block_images_info, tile_info, mapObjIdToImageId;
    var mapObjIdToItemId;
    var mapItemIdToObjectId;
    var new_item;
    async.waterfall([
        // 0. 情報取得フェーズ
        function(callback) {
            // オブジェクトの情報取得
            network.getObjectAttrInfo( client, callback );
        },
        function(_object_attr_info, _moto_object_attr_info, callback) {
            object_attr_info = _object_attr_info;
            moto_object_attr_info = _moto_object_attr_info;
            // イメージ情報
            network.getDungeonImageBlock(client, callback );
        },
        function(_block_images_info, callback) {
            block_images_info = _block_images_info;

            // タイル情報取得
            network.getTileList( client, callback );
        },
        function(_tile_info, callback ){
            tile_info = _tile_info;
            mapObjIdToImageId = dungeon.makeMapObjIdToImageIdFromTileInfo(tile_info);
            callback();
        },
        function(callback) {
            network.getDungeon( client, user_id, 0, callback );
        },
        function(_dungeon_info, callback) {
            dungeon_info = _dungeon_info;

            // 1. ダンジョンの空きスペースをチェックする
            rest_item_cnt = dungeon.getDungeonSpaceCnt( dungeon_info, object_attr_info );
            if (rest_item_cnt-1/*階段分*/ == 0 ) {
                callback("アイテムを置くためのスペースがありません。ダンジョンを広げてください")
            }
            else {
                callback();
            }
        },
        function(callback) {
            // 2. 大陸に入り口を設置
            network.getDungeon( client, 0, 0, callback ); // 大陸
        },
        function(_island_info, callback) {
            island_info = _island_info; // 大陸内の自分の領地のある範囲
            network.getIslandGroundInfoByUser( client, user_id, callback );
        },
        function(_island_ground_info, callback) {
            island_ground_info = _island_ground_info;
            dungeon.setDungeonEntracne( island_info, island_ground_info, object_attr_info, mapObjIdToImageId);
            var island_mes = network.makeSetDungeon();
            island_mes.user_id = 0; // 大陸 
            island_mes.dungeon_info = island_info;
            island_mes.images = block_images_info;
            island_mes.object_info = moto_object_attr_info;
            island_mes.tile_info = tile_info;
            network.setDungeon( client, island_mes, callback );
        },
        function(callback) {
            // 3. 情報を登録する
            network.createNewItem( client, text, false/*no monster*/, moto_object_attr_info, callback );
        },
        function(_new_item, callback) {
            new_item = _new_item;
            // ダンジョンを登録し直さないと更新されないので
            var dungeon_mes = network.makeSetDungeon();
            dungeon_mes.user_id = user_id;
            dungeon_mes.dungeon_info = dungeon_info;
            dungeon_mes.images = block_images_info;
            dungeon_mes.object_info = moto_object_attr_info;
            dungeon_mes.tile_info = tile_info;
            network.setDungeon( client, dungeon_mes, callback );
        },
        function(callback) {
            network.getObjectAttrInfo( client, callback );
        },
        function(_object_attr_info, _moto_object_attr_info, callback) {
            object_attr_info = _object_attr_info;
            moto_object_attr_info = _moto_object_attr_info;
            mapItemIdToObjectId = dungeon.makeMapItemIdToObjIdFromObjectAttrInfo( object_attr_info );

            // イメージ情報
            network.getDungeonImageBlock(client, callback );
        },
        function(_block_images_info, callback) {
            block_images_info = _block_images_info;

            // タイル情報取得
            network.getTileList( client, callback );
        },
        function(_tile_info, callback ){
            tile_info = _tile_info;
            mapObjIdToImageId = dungeon.makeMapObjIdToImageIdFromTileInfo(tile_info);

            // 4. ダンジョン内に情報を配置する
            network.getDungeon( client, user_id, 0, callback );
        },
        function(_dungeon_info, callback) {
            dungeon_info = _dungeon_info;
            if ( !dungeon.setItemToEmptyArea( dungeon_info, object_attr_info, mapItemIdToObjectId, new_item) ) {
                callback("ダンジョン内に情報を置けませんでした");
                return;
            }
            var dungeon_mes = network.makeSetDungeon();
            dungeon_mes.user_id = user_id;
            dungeon_mes.dungeon_info = dungeon_info;
            dungeon_mes.images = block_images_info;
            dungeon_mes.object_info = moto_object_attr_info;
            dungeon_mes.tile_info = tile_info;
            network.setDungeon( client, dungeon_mes, callback );
        } 
    ], function(err) {
        if ( !err ) err = "記事を書き込みました";
        res.render('write_info', {error_message:err, rest_item_cnt:rest_item_cnt, content:text});
    });
}

// 記事の変更処理
exports.modify_info = function(req,res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    var info_id = req.query.info_id;
    var view_id = user_id; // 同義として扱う

    if ( !info_id || !view_id ) {
        gotoTopPage(res);
        return;
    }

    var aitem;
    async.waterfall([
        function(callback) {
            network.getItemInfoByUserId(client, view_id, callback);
        }, 
        function(_iteminfo, callback) {
            for(var it2 in _iteminfo.aitem_dic ) {
                var dic = _iteminfo.aitem_dic[it2];
                var itemid = dic.aitem.item_id;
                if ( itemid == info_id ) {
                    aitem = dic.aitem; // AItemの情報が入る
                    break;
                }
                // 一個しか読まないけれども
            }
            //download_folder = path.join( global.DOWNLOAD_FOLDER, ""+itemid );
            callback();
        }
    ], function(err, article_content) {
        res.render('modify_info', {error_message:err, aitem:aitem, info_id:info_id, view_id:view_id});
    });
}

// 記事の変更処理のポスト処理
exports.modify_info_post = function(req,res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    var info_id = req.body.info_id;
    var view_id = user_id; // 同義として扱う
    var new_text = req.body.inputtext;

    if ( !info_id || !view_id || new_text === undefined) {
        gotoTopPage(res);
        return;
    }

    var aitem;
    async.waterfall([
        function(callback) {
            // まずはアイテム情報を得る
            network.getItemInfoByUserId(client, view_id, callback);
        }, 
        function(_iteminfo, callback) {
            for(var it2 in _iteminfo.aitem_dic ) {
                var dic = _iteminfo.aitem_dic[it2];
                var itemid = dic.aitem.item_id;
                if ( itemid == info_id ) {
                    aitem = dic.aitem; // AItemの情報が入る
                    // 一個しか読まないけれども
                    break;
                }
            }
            //download_folder = path.join( global.DOWNLOAD_FOLDER, ""+itemid );
            callback();
        },
        function(callback) {
            // アイテム情報を変更する
            aitem.header_string = new_text; // HTML更新用
            network.changeAItem( client, aitem.item_id, aitem.item_image_id, new_text, callback );
        }
    ], function(err) {
        res.render('modify_info', {error_message:err, aitem:aitem, info_id:info_id, view_id:view_id});
    });
}

// ファイルアップロード
exports.upload_file = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    //res.render('index' );
    res.redirect('info');
}

// ファイルアップロード処理のテスト
exports.upload_file_post = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var info_id = req.query.info_id;
    var view_id = req.query.view_id;
    if ( !info_id || !view_id ) {

        res.redirect("index");
        return;
    }
    var dir_base = path.join( global.DOWNLOAD_FOLDER, ""+info_id );
    
    req.busboy.on('file', function(fieldname, file, filename, encoding, mimetype ) {
        var saveTo = path.join(dir_base, path.basename(filename) );
        if ( filename === undefined ) {
            saveTo = path.join( os.tmpDir(), "nothing" );
        }
        console.log(saveTo);
        file.pipe(fs.createWriteStream(saveTo));
    });
    req.busboy.on('field', function(key, value, keyTruncated, valueTruncated) {
//        console.log("fileld " + key + ":"+ value );
    });
    req.busboy.on('finish', function() {
        res.redirect('info?info_id='+info_id+'&view_id='+view_id );
    });
    req.pipe(req.busboy);
}

// ファイルを削除する
exports.delete_file_post = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var info_id = req.query.info_id;
    var view_id = req.query.view_id;
    var filename = req.query.filename;
    if ( !info_id || !view_id ) {

        res.redirect("index");
        return;
    }

    var dir_base = path.join( global.DOWNLOAD_FOLDER, ""+info_id );
    filename = path.normalize(filename).replace("..", "");
    var delete_path = path.join( dir_base, filename );
    fs.unlink( delete_path, function() {
        console.log( "delete file "+ delete_path);
        res.redirect('info?info_id='+info_id+'&view_id='+view_id );
    });
}

// ログアウト処理
exports.logout = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    //var item_id = req.body.item_id;

    // ログアウト処理
    network.closeGodaiQuestServer(client);
    res.render('index', { error_message: "ログアウトしました"});
}

// ユーザ登録
exports.register_user = function(req, res) {

    res.render('register_user');
}

// クライアントアドレスを得る
function getClientAddress (req) {
        return (req.headers['x-forwarded-for'] || '').split(',')[0] 
        || req.connection.remoteAddress;
};

// ユーザ登録実行
exports.register_user_post = function(req, res)  {

    var email;
    var password;
    var name;

    var str = "abcdefghijklmnopqrstuvwxyz";
    function randCh() {
       return str[ Math.floor(Math.random()*str.length) ];
    }
    var tmpfile = randCh() + randCh() + randCh() + randCh();
    var tmpfile2 = randCh() + randCh() + randCh() + randCh();
    var tmpfilepath = path.join( os.tmpdir(), tmpfile );
    var tmpfilepath2 = path.join( os.tmpdir(), tmpfile2 );
    var receive_flag = false;

    req.busboy.on('file', function(fieldname, file, filename, encoding, mimetype ) {
        console.log("temp file: " + tmpfilepath);
        receive_flag = true;
        file.pipe(fs.createWriteStream(tmpfilepath));
    });
    req.busboy.on('field', function(key, value, keyTruncated, valueTruncated) {
        if ( key == "email" ) email = value;
        if ( key == "password" ) password = value;
        if ( key == "name" ) name = value;
//        console.log("fileld " + key + ":"+ value );
    });
    req.busboy.on('finish', function() {
        if ( !receive_flag ) { callback("画像ファイルを指定してください"); return; }
        async.waterfall([
            // イメージ画像をリサイズする
            function(callback) {
                im.resize({ 
                    srcPath: tmpfilepath,
                    dstPath: tmpfilepath2,
                    width: 64,
                    height: 64
                }, callback );
            },
            function(stdout, stderr, callback) {
                network.addUser( email, password, name, tmpfilepath2, "c:\\tmp\\godaiquest", getClientAddress(req), callback );
            },
            function(callback) {
                // ファイル削除
                fs.unlink( tmpfilepath, callback );
            },
            function(callback) {
                // ファイル削除
                fs.unlink( tmpfilepath2, callback );
            }
        ], function(err) {
            if ( err && !err.length ) {
                err = "イメージファイルではありません";
            }
            if ( err ) {
                res.render('register_user', {error_message:err} );
            }
            else {
                res.render('index', {error_message:"ユーザ登録に成功しました"});
            }
        });
    });
    req.pipe(req.busboy);
    
}

