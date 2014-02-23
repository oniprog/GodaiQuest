// ネットワーク、サーバとの接続関連
var async = require('async');
var net = require('net');
var ProtoBuf = require('protobufjs');
var crypto = require('crypto');
var filegqs = require('./filegqs');
var path = require('path');
var zlib = require('zlib');
var fs = require('fs');
var Int64 = require('node-int64');

//
var CLIENT_VERSION = 2014021819;

// コマンド
var COM_AddUser = 1;
var COM_TryLogon = 2;
var COM_GetDungeon = 3;
var COM_SetDungeon = 4;
var COM_GetDungeonBlockImage = 5;
var COM_SetDungeonBlockImage = 6;
var COM_GetTilePalette = 7;
var COM_SetTilePalette  =8;
var COM_GetObjectAttrInfo  = 9;
var COM_SetObjectAttrinfo = 10;
var COM_GetTileList = 11;
var COM_GetUserInfo = 12;
var COM_SetAItem = 13;
var COM_GetItemInfo = 14;
var COM_GetAItem = 15;
var COM_SetAUser = 16;
var COM_ChangeAItem = 17;
var COM_UploadItemFiles = 18;
var COM_Polling = 19;
var COM_GetIslandGroundInfo = 20;
var COM_ChangeObjectAttr = 21;
var COM_ChangeDungeonBlockImagePair = 22;
var COM_GetLocationInfo = 23;
var COM_GetMessageInfo  = 24;
var COM_SetAMessage = 25;
var COM_GetExpValue = 26;
var COM_GetUnpickedupItemInfo =27;
var COM_GetAshiato = 28;
var COM_GetAshiatoLog = 29;
var COM_GetArticleString = 30;
var COM_SetItemArticle  = 31;
var COM_ReadArticle  =32;
var COM_DeleteLastItemArticle = 33;
var COM_UseExperience = 34;
var COM_GetDungeonDepth  = 35;
var COM_GetMonsterInfo  =36;
var COM_SetMonster  = 37;
var COM_GetRDReadItemInfo = 38;
var COM_SetRDReadItem = 39;
var COM_ChangePassword = 40;
var COM_SetUserFolder  = 41;
var COM_GetUserFolder  = 42;
var COM_GetRealMonsterSrcInfo =43;
var COM_GetItemInfoByUserId = 44;

//
var builder = ProtoBuf.loadProtoFile("routes/godaiquest.proto");
var LoginMessage = builder.build("godaiquest.Login");
var UserInfoMessage = builder.build("godaiquest.UserInfo");
var ItemInfoMessage = builder.build("godaiquest.ItemInfo");
var ItemArticleMessage = builder.build("godaiquest.ItemArticle");

// ロック処理のコールバックリスト 
var listLockCallback = [];
var lockedConn = false;

// ロック処理（同時にアクセスしないように)
function lockConn(callback) {

    if ( !lockedConn ) {
        // ロックの取得できた
        lockedConn = true;
        callback();
    }
    else {
        // ロック取得できなかった
        listLockCallback.push( callback );
    }
}
// ロック解除処理
function unlockConn() {

    if ( !lockedConn ) return;
    if ( listLockCallback.length == 0 ) {
        lockedConn = false;
    }
    else {
        var callback = listLockCallback.splice(0, 1)[0];
        callback();
    }
}

// Dword送信
function writeDword(client, dword, callback) {

    var buf = new Buffer(4);
    buf[3] = dword & 0xff;
    buf[2] = (dword >> 8) & 0xff;
    buf[1] = (dword >> 16) & 0xff;
    buf[0] = (dword >> 24) & 0xff;

    if ( callback ) 
        client.write(buf, callback);
    else
        client.write(buf);
}

// Dword送信
function writeDwordRev(client, dword, callback) {

    var buf = new Buffer(4);
    buf[0] = dword & 0xff;
    buf[1] = (dword >> 8) & 0xff;
    buf[2] = (dword >> 16) & 0xff;
    buf[3] = (dword >> 24) & 0xff;

    if ( callback ) 
        client.write(buf, callback);
    else
        client.write(buf);
}

// プロトコルバッファのオブジェクトの受信用
function readProtoMes( client, callback ) {

    setReadCallback( client, 4,function(err) {
        if ( err ) callback(err);
        else {
            var readbyte = readDwordRev( client );
            setReadCallback( client, readbyte, function(err) {

                if ( err) callback( err );
                else {
                    var ret = client.read_buffer.slice(0, readbyte);
                    client.read_buffer = client.read_buffer.slice(readbyte);
                    callback( null, ret );
                }
            });
        }
    });
}
// Byte受信
function readByte( client ) {
    var ret = client.read_buffer.readUInt8(0);
    client.read_buffer = client.read_buffer.slice(1);
    return ret;
}

// Byte書き込み
function writeByte( client, data ) {
    var buf = new Buffer(1);
    buf.writeUInt8( data, 0 );
    client.write(buf);
}

// Dword受信
function readDword( client ) {
    var ret = client.read_buffer.readInt32BE(0);
    client.read_buffer = client.read_buffer.slice(4);
    return ret;
}

// Dword受信
function readDwordRev( client ) {
    var ret = client.read_buffer.readInt32LE(0);
    client.read_buffer = client.read_buffer.slice(4);
    return ret;
}

// プロトコルバッファのメッセージ送信用 
function writeProtoMes( client, mes ) {
    var buf = mes.toBuffer();
    writeDwordRev( client, buf.length );
    client.write( buf );
}

// 文字列送信
function writeString( client, str ) {

    var buf = new Buffer( str, "ucs2" );
    writeLength( client, buf.length );
    client.write(buf);
}

// ファイル情報を送る
// { path:relativePath, fullpath:filepath, size:stats.size}
function writeFileInfoSub( client, fileinfo) {

    writeDword( client, fileinfo.size );
    writeString( client, fileinfo.path );
}

// ファイル情報を送る
function writeFileInfo( client, listFiles) {

    for(var it in listFiles) {
        var afile = listFiles[it];
        writeFileInfoSub( client, afile );
    }
    writeDword( client, -1);
}

// 文字列を得る
function readString( client, callback ) {

    var length_str;
    async.waterfall([
        function(callback) {
            readLength(client, callback );
        },
        function(_length_str, callback) {
            length_str = _length_str;
            setReadCallback( client, length_str, callback );
        },
        function(callback) {
            var str_ret = client.read_buffer.slice(0, length_str);
            client.read_buffer = client.read_buffer.slice(length_str);
            callback( null, str_ret.toString("ucs2") );
        }
    ], function(err, str_ret ) {
        callback( null, str_ret );
    });
}

// バイナリをよむ
function readBinary( client, callback ) {

    var length_binary;
    async.waterfall([
        function(callback) {
            readLength(client, callback );
        },
        function(_length_binary, callback) {
            length_binary = _length_binary;
            setReadCallback( client, length_binary, callback );
        },
        function(callback) {
            var ret = client.read_buffer.slice(0, length_binary);
            client.read_buffer = client.read_buffer.slice(length_binary);
            callback( null, ret );
        }
    ], function(err, ret ) {
        callback( null, ret );
    });
}


// ファイルを受信する
function readFile( client, dir_base, callback ) {

    var filepath;
    var filename;
    var outcallback = callback;
    var outfd, data;
    async.waterfall( [
        function(callback) {
            setReadCallback( client, 1, callback );
        },
        function(callback) {
            var end_code = readByte( client );
            if ( end_code == 0 ) {
                outcallback( null, 0);
            }
            else {
                callback();
            }
        },
        function(callback) {
            readString( client, callback );
        },
        function(_filename, callback) {
            filename = _filename;
            filepath = path.join( dir_base, filename );
            readBinary( client, callback );
        },
        function(data, callback) {
            zlib.gunzip( data, callback );
        },
        function(_data, callback ) {
            data = _data;
            console.log("receive file : " + filepath );
            fs.open( filepath, "w", callback );
        },
        function(_outfd, callback) {
            outfd = _outfd;
            fs.write( outfd, data, 0, data.length, null, callback );
        },
        function(write_byte, buf, callback) {
            fs.close( outfd, callback );
        }
    ], function(err) {
        callback(err, 1 );
    });
}
// ファイル群を送付する
function readFiles( client, dir_base, callback ) {

    readFile( client, dir_base, function(err, end_code) {
        if ( err ) { callback(err); }
        else {
            if ( end_code == 0 ) callback();
            else readFiles( client, dir_base, callback );
        }
    });
}

// 長さ取得を行う
// 最初のバイトに、長さが埋め込まれている
function readLength( client, callback ) {
    setReadCallback( client, 1, function(err) {
        if ( err ) { callback(err); }
        else {
            var ch1 = readByte( client );
            if( ch1 < 0x10 ) {
                callback( null, ch1 & 0x0f );
                return;
            }
            var req_byte = (ch1 & 0xf0) >> 4; 
            setReadCallback( client, req_byte, function(err) {
                if ( err ) { callback(err); }
                else {
                    var ret = (ch1 & 0x0f);
                    if ( req_byte-- > 0 ) { ret *= 0x100; ret += readByte( client ); }
                    if ( req_byte-- > 0 ) { ret *= 0x100; ret += readByte( client ); }
                    if ( req_byte-- > 0 ) { ret *= 0x100; ret += readByte( client ); }
                    callback( err, ret );
                }
            });
        }
    });
}

// 長さを送信する
function writeLength( client, length ) {

    if ( length < 0x10 ) {
        var buf = new Buffer(1);
        buf.writeUInt8(length, 0);
        client.write(buf);
    }
    else if (length < 0xfff ) { 
        var buf = new Buffer(2);
        buf.writeUInt8((length >> 8) | 0x10, 0 );
        buf.writeUInt8( length & 0xff, 1 );
        client.write(buf);
    }
    else if ( length < 0xfffff ) {
        
        var buf = new Buffer(3);
        buf.writeUInt8((length >> 16) | 0x20, 0 );
        buf.writeUInt8((length >> 8) & 0xff, 1 );
        buf.writeUInt8( length & 0xff, 2 );
        client.write(buf);
    }
    else {

        var buf = new Buffer(4);
        buf.writeUInt8((length >> 24) | 0x30, 0 );
        buf.writeUInt8((length >> 16) & 0xff, 1 );
        buf.writeUInt8((length >> 8) & 0xff, 2 );
        buf.writeUInt8( length & 0xff, 3 );
        client.write(buf);
    }
}

// データ読み込みのCallbackを設定する
function setReadCallback( client, length, callback ) {

    if ( client.read_buffer.length >= length ) {
        callback();
        return;
    }

    function callback_receive(chunk) {
        if ( client.read_buffer.length >= length ) {
            client.removeListener('data', callback_receive );
            callback();
        }
    }
    client.on('data', callback_receive );
}

// コマンドの実行結果を受け取る
function readCommandResult( client, callback ) {
    setReadCallback( client, 4, callback );
}

// 接続を切る
function closeGodaiQuestServer(client) {

    var num = client.number;
    delete global.connect_gqs[num];
    client.destroy();
}

// クライアントを得る
// 接続時に自動登録される
function getClient(client_number) {
    if ( !client_number ) return null;
    var client = global.connect_gqs[client_number];
    return client;  // クローズ時に自動削除されるはずなので
}

// Godai Questサーバへのログイン処理を行う
function connectGodaiQuestServer(mailaddress, password, callback) {

    var client = new net.Socket();
    client.read_buffer = new Buffer(0);
    client.number = global.connect_num++;

    client.on('data', function(chunk) {
        client.read_buffer = Buffer.concat( [client.read_buffer, chunk] );
     });

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            client.connect('21014', 'localhost', callback );
        },
        function(callback) {
             // connected
            global.connect_gqs[client.number] = client;
            readCommandResult(client, callback);
            writeDword( client, 0 ); // Version
        },
        function(callback) {
            // read dword
            var okcode = readDword( client );
            if (okcode != 1 ) {
                callback("Godai Quest Server接続失敗 ");
            }
            else {

                // senc logon command
                writeDword( client, COM_TryLogon );
                writeDword( client, 0 ); // version
                var login = new LoginMessage();
                login.mail_address = mailaddress;
                var passwordhash = crypto.createHash('sha512').update( password ).digest('hex');
                login.password = passwordhash;
                login.client_version = CLIENT_VERSION;
                writeProtoMes( client, login );

                readCommandResult(client, callback );
            }
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                closeGodaiQuestServer(client);
                callback("ログインに失敗しました")
            }
            else {
                readCommandResult( client, callback );
            }
        },
        function(callback) {
            var userId = readDword( client );
            callback( null, userId, client );
        }
    ], function(err, userId, client ) {
        unlockConn();
        callback(err, userId, client);
    });
    client.on('close', function() {

        closeGodaiQuestServer(client);
    });
    client.on('error', function(err) {
        callback("Godai Quest Serverが起動していません "+err)
    });
    return client;
 }

// URI Imageに変換する
function convURIImage( image ) {
    return "data:image/png;base64," + image.toString('base64');
}


// Godai Questサーバーからユーザー一覧を得る
/*
message AUser {

	optional int32 user_id = 1;
	optional string mail_address = 2;
	optional string user_name = 3;
	optional bytes user_image = 4;
}
message AUserDic {

	optional int32 index = 1;
	optional AUser auser = 2;
}
message UserInfo {

	repeated AUserDic uesr_dic = 1;
}
*/
function getAllUserInfo(client, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetUserInfo );
            writeDword( client, 0 ); // Version
            readCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 )
                callback( "ユーザ一覧の取得に失敗しました" )
            else {
                readProtoMes( client, callback );
            }
        },
        function(data, callback) {
            var userinfo = UserInfoMessage.decode(data);

            for( var it in userinfo.uesr_dic ) {
                var auser = userinfo.uesr_dic[it].auser;
                auser.uri_image = convURIImage( auser.user_image );
            }
            
            callback( null, userinfo );
        }
    ], function(err, userinfo) {
        unlockConn();
        callback( err, userinfo );
    });
}

// 未取得アイテム数取得
function getUnpickedupItemInfo(client, userId, dungeonId, callback) {

    var list_length = 0;
    async.waterfall( [
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetUnpickedupItemInfo );
            writeDword( client, 0 ); // Version
            writeDword( client, userId );
            writeDword( client, dungeonId );
            readCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("未取得アイテム情報の取得に失敗しました");
            }
            else {
                readLength( client, callback );
            }
        },
        function( _list_length, callback ) {
            list_length = _list_length;
            setReadCallback( client, 4 * list_length, callback );
        },
        function( callback ) {
            listItemId = [];
            for( var it=0; it<list_length; ++it ) {
                var Id = readDword( client );
                listItemId.push( Id );
            }
            callback( null, listItemId );
        }
    ], function(err, listItemId ) {
        unlockConn();
        callback( err, listItemId );
    });
}

// アイテム情報を得る
/*
  message AItem {

	optional int32 item_id  = 1;
	optional int32 item_image_id = 2;
	optional string header_string = 3;
	optional bytes header_image = 4;
	optional bool bNew = 5;
}

message AItemDic {
	optional int32 index = 1;
	optional AItem aitem = 2;
}
message ItemInfo {

	repeated AItemDic aitem_dic = 1;
}
*/
function getItemInfo( client, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetItemInfo );
            writeDword( client, 0 );  // version
            readCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("アイテム情報の取得に失敗しました");
            }
            else {
                readProtoMes( client, callback );
            }
        },
        function(data, callback) {
            var iteminfo = ItemInfoMessage.decode(data);
            callback( null, iteminfo );
       }
    ], function(err, iteminfo) {
        unlockConn();
        callback(err, iteminfo );
    });
}
function getItemInfoByUserId( client, user_id, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetItemInfoByUserId );
            writeDword( client, 0 );  // version
            writeDword( client, user_id );
            readCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("アイテム情報の取得に失敗しました");
            }
            else {
                readProtoMes( client, callback );
            }
        },
        function(data, callback) {
            var iteminfo = ItemInfoMessage.decode(data);
            callback( null, iteminfo );
        }
    ], function(err, iteminfo) {
        unlockConn();
        callback(err, iteminfo );
    });
}

// 読み込んだことを記録する(特殊ダンジョン用かな)
function readMarkArticle( client, user_id, item_id, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_ReadArticle );
            writeDword( client, 0 ); // version
            writeDword( client, item_id );
            writeDword( client, user_id );
            readCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ){
                callback("アイテムを読んだことを記録できなかった");
            }
            else {
                callback();
            }
        }
    ], function(err) {
        unlockConn();
        callback(err);
    });
}

// アイテム情報をダウンロードする（あと、読んだことにする)
function getAItem(client, item_id, callback) {

    var download_folder = path.join( global.DOWNLOAD_FOLDER, ""+item_id );

    var listFiles;
    async.waterfall( [
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            // フォルダ作成
            fs.exists( download_folder, function(exists) {
                if ( !exists ) 
                    fs.mkdir( download_folder, callback );
                else
                    callback();
            });
        },
        function(callback) {
            // ファイルリストを得る
            filegqs.getFileList( download_folder, callback );
        },
        function(_listFiles, callback) {
            listFiles = _listFiles;
            writeDword( client, COM_GetAItem );
            writeDword( client, 2 ); // version
            writeDword( client, item_id );
            // ファイル情報を送信する
            writeFileInfo( client, listFiles );
            // ファイルの受信
            readFiles( client, download_folder, callback );
        },
        function(callback) {
            // ファイルリストを得る(再)
            filegqs.getFileList( download_folder, callback );
        }
    ], function(err, listFiles) {
        unlockConn();
        callback(err, listFiles);
    });
}

// 記事を読む
function getArticleString( client, item_id, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            
            writeDword( client, COM_GetArticleString );
            writeDword( client, 0 ); // version
            writeDword( client, item_id );
            readCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("記事への書き込みの取得に失敗しました");
            }
            else {
                callback();
            }
        },
        function(callback) {
            readString( client, callback );
        }
    ], function(err, article_content) {
        unlockConn();
        callback(err, article_content);
    });
}

// 記事の書き込み
/*
  message ItemArticle {

	optional int32 item_id = 1;
	optional int32 article_id = 2;
	optional int32 user_id = 3;
	optional string contents = 4;
	optional sfixed64 cretae_time = 5;
}*/
function setItemArticle( client, item_id, article_id, user_id, contents, callback ) {

    async.waterfall( [
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_SetItemArticle );
            writeDword( client, 0 ); // version
            var item_article = new ItemArticleMessage();
            item_article.item_id = item_id;
            item_article.article_id = article_id;
            item_article.user_id = user_id;
            item_article.contents = contents;
            item_article.create_time = new Int64(0);
            writeProtoMes( client, item_article );

            readCommandResult( client, callback );
        },
        function( callback ) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("記事内容の書き込みに失敗しました");
            }
            else {
                callback();
            }
        }
    ] , function(err) {
        unlockConn();
        callback(err);
    });
}

// 記事の最後の書き込みを削除する
function deleteLastItemArticle(client, item_id, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_DeleteLastItemArticle );
            writeDword( client, 0 ); // version
            writeDword( client, item_id );
            readCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback( "記事の削除に失敗しました");
            }
            else {
                callback();
            }
        }
    ], function(err) {
        unlockConn();
        callback(err);
    });
}

module.exports = {
    writeDword: writeDword,
    getClient : getClient,
    connectGodaiQuestServer:connectGodaiQuestServer,
    closeGodaiQuestServer: closeGodaiQuestServer,
    getAllUserInfo : getAllUserInfo,
    getUnpickedupItemInfo : getUnpickedupItemInfo,
    getItemInfo : getItemInfo,
    getItemInfoByUserId : getItemInfoByUserId,
    readMarkArticle : readMarkArticle,
    getAItem: getAItem,
    getArticleString: getArticleString,
    setItemArticle: setItemArticle,
    deleteLastItemArticle: deleteLastItemArticle
}
