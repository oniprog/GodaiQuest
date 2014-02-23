var network = require("./network");
var async = require("async"); 
var filegqs = require("./filegqs");
var path = require('path');
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
function gotoTopPage() {
    res.render('index', {error_messsage: "ログインしてください"});
}
    

// ログイン処理
exports.login = function(req, res){

    var email = req.body.mailaddress;
    var password = req.body.password;

    network.connectGodaiQuestServer(email, password, function(err, user_id, client) {
        if ( err ) {
            res.render('index', { error_message: err, mailaddress:email });
        }
        else {
            req.session.user_id = user_id;
            req.session.client_number = client.number;
            res.redirect('user_list');
        }
    });
//    res.redirect('list');
};

//
function checkLogin( req, res ) {

    // 接続チェック
    if( !req.session.user_id ) {
        res.render('index', {error_messsage: "ログインしてください"});
        return null;
    }
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
//            network.getAllUserInfo(client, callback);

            getUserInfo( client, req, callback);
        },
        function(_userinfo, callback) {
            userinfo = _userinfo;
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

// 未読の情報一覧表示
var LIST_COUNT = 10;
exports.info_list = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    var view_id = req.query.view_id;
    var index = req.query.index;
    if ( !index ) index = 0;
    
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
        gotoTopPage();
        return;
    }
    var index = req.query.index;
    if ( !index ) index = 0;

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
        gotoTopPage();
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
            console.log(info_id);
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
        res.render('info', {error_message:err, iteminfo:iteminfo, info_id:info_id, view_id:view_id, listFiles:listFiles, article_content: article_cont, pagesize:LIST_COUNT});
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
        gotoTopPage();
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
        gotoTopPage();
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

// ログアウト処理
exports.logout = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    // ログアウト処理
    network.closeGodaiQuestServer(client);
    res.render('index', { error_message: "ログアウトしました"});
}