var async = require('async');
var net = require('net');
var ProtoBuf = require('protobufjs');
var crypto = require('crypto');

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
    var ret = client.read_buffer.readInt8(0);
    client.read_buffer = client.read_buffer.slice(1);
    return ret;
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

// 長さ取得を行う
// 最初のバイトに、長さが埋め込まれている
function readLength( client, callback ) {
    setReadCallback( client, 1, function(err) {
        if ( err ) { callback(err); }
        else {
            var ch1 = readByte( client );
            if( ch1 < 0x10 ) {
                callback( null, ch1 );
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


module.exports = {
    writeDword: writeDword,
    getClient : getClient,
    connectGodaiQuestServer:connectGodaiQuestServer,
    getAllUserInfo : getAllUserInfo,
    getUnpickedupItemInfo : getUnpickedupItemInfo,
    getItemInfo : getItemInfo,
    getItemInfoByUserId : getItemInfoByUserId
}
        