//
// ネットワーク、サーバとの接続関連
//
var Long = require('long');
var async = require('async');
var net = require('net');
var ProtoBuf = require('protobufjs');
var crypto = require('crypto');
var filegqs = require('./filegqs');
var path = require('path');
var zlib = require('zlib');
var fs = require('fs');
var Int64 = require('node-int64');
var dungeon = require('./dungeon');

// 通信のクライアントとしてのバージョン番号 
var CLIENT_VERSION = 2014021819;

// GQSのホスト名
var HOST = "localhost";
// GQSのポート
var PORT = 21014;

// dungeonよりコピーした
// ここに置きたくないのだけれども
var COMMAND_Nothing = 0; 
var COMMAND_GoUp = 1;
var COMMAND_GoDown = 2;
var COMMAND_IntoDungeon = 3;
var COMMAND_GoOutDungeon = 4;

// GQS(Godai Quest Server)の通信コマンド
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

// プロトコルバッファの生成用オブジェクト
// godaiquest.proto参照のこと
var builder = ProtoBuf.loadProtoFile("routes/godaiquest.proto");
var LoginMessage = builder.build("godaiquest.Login");
var UserInfoMessage = builder.build("godaiquest.UserInfo");
var ItemInfoMessage = builder.build("godaiquest.ItemInfo");
var ItemArticleMessage = builder.build("godaiquest.ItemArticle");
var GetDungeonMessage = builder.build("godaiquest.GetDungeon");
var DungeonInfoMessage = builder.build("godaiquest.DungeonInfo");
var SetDungeonMessage = builder.build("godaiquest.SetDungeon");
var DungeonBlockImageInfoMessage = builder.build("godaiquest.DungeonBlockImageInfo");
var ObjectAttrInfoMessage = builder.build("godaiquest.ObjectAttrInfo");
var ObjectAttrDicMessage = builder.build("godaiquest.ObjectAttrDic");
var ObjectAttrMessage = builder.build("godaiquest.ObjectAttr");

var TileInfoMessage = builder.build("godaiquest.TileInfo");
var IslandGroundInfoMessage = builder.build("godaiquest.IslandGroundInfo");
var ImagePairMessage = builder.build("godaiquest.ImagePair");
var AItemMessage = builder.build("godaiquest.AItem");


// ロック時のコールバック格納用
// lockCcon, unlockConn
var listLockCallback = [];
// ロックしているときtrue
// lockCcon, unlockConn
var lockedConn = false;

// ロック処理
// 複数のプロセスが同時にネットワークにアクセスしないように制限する。
// 再入には対応していない
function lockConn(callback) {

    if ( !lockedConn ) {
        // ロックの取得できた
        lockedConn = true;
        callback();
    }
    else {
        // ロック取得できなかった
        // すぐに返るけれども、あとで呼べるようにしておく
        listLockCallback.push( callback );
    }
}

// ロック解除処理
function unlockConn() {

    // ロックしてなければ何もしない
    if ( !lockedConn ) return;
    if ( listLockCallback.length == 0 ) {
        // ロック解除
        lockedConn = false;
    }
    else {
        // 保存してあるコールバックを呼ぶ
        var callback = listLockCallback.splice(0, 1)[0];
        callback();
    }
}

// Dword送信
// callback引数は省略可能
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

// Dword送信(逆向きにして)
// callback引数は省略可能
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

    // 4バイト受信することを確実にする
    ensureReadByte( client, 4,function(err) {
        if ( err ) callback(err);
        else {
            // バイト数を読み込む
            var readbyte = readDwordRev( client );
            // バイト数を受信することを確実にする
            ensureReadByte( client, readbyte, function(err) {

                if ( err) callback( err );
                else {
                    // 読み込んだデータを取り出して渡す
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

// バイナリを保存する
function writeBinary( client, data ) {
    var buf = new Buffer( data );
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

    // ファイルサイズ
    // -1ならば終了
    writeDword( client, fileinfo.size );
    // ファイル名
    writeString( client, fileinfo.path );
}

// ファイル情報を送る
function writeFileInfo( client, listFiles) {

    for(var it in listFiles) {
        var afile = listFiles[it];
        writeFileInfoSub( client, afile );
    }
    // -1で終了する 
    writeDword( client, -1);
}

// 文字列を得る
function readString( client, callback ) {

    var length_str;
    async.waterfall([
        function(callback) {
            // 長さを得る
            readLength(client, callback );
        },
        function(_length_str, callback) {
            // 読み込みを保証する
            length_str = _length_str;
            ensureReadByte( client, length_str, callback );
        },
        function(callback) {
            // 文字列本体を得る
            var str_ret = client.read_buffer.slice(0, length_str);
            client.read_buffer = client.read_buffer.slice(length_str);
            // utf-8に変換する
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
            ensureReadByte( client, length_binary, callback );
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
            // 継続フラグを受信する
            ensureReadByte( client, 1, callback );
        },
        function(callback) {
            // 継続するかのチェック
            var end_code = readByte( client );
            if ( end_code == 0 ) {
                // 終了する
                outcallback( null, 0);
            }
            else {
                callback();
            }
        },
        function(callback) {
            // ファイル名を得る
            readString( client, callback );
        },
        function(_filename, callback) {
            filename = _filename;
            filepath = path.join( dir_base, filename );
            // バイナリを読む
            readBinary( client, callback );
        },
        function(data, callback) {
            // 展開する
            zlib.gunzip( data, callback );
        },
        function(_data, callback ) {
            // ファイルを開いて保存する
            data = _data;
            console.log("receive file : " + filepath );
            fs.open( filepath, "w", callback );
        },
        function(_outfd, callback) {
            // ファイルに書き込み 
            outfd = _outfd;
            fs.write( outfd, data, 0, data.length, null, callback );
        },
        function(write_byte, buf, callback) {
            // ファイルをクローズする
            fs.close( outfd, callback );
        }
    ], function(err) {
        // 継続を渡す
        callback(err, 1 );
    });
}
// ファイル群を送付する
function readFiles( client, dir_base, callback ) {

    // ファイルを読む 
    readFile( client, dir_base, function(err, end_code) {
        if ( err ) { callback(err); }
        else {
            // 継続時は自分自身を再帰呼び出しする
            if ( end_code == 0 ) callback();
            else readFiles( client, dir_base, callback );
        }
    });
}

// 長さ取得を行う
//
// 最初のバイト(f1b)に、長さが埋め込まれている
//
// f1b < 0x10   そのまま受信バイト数
//
// fib >= 0x10   (f1b & 0xf0) >> 4が連続バイト数
//
// f1b & 0x0fが上位部分になり、連続バイト数を受信して足す          
//
// writeLengthも見よ
function readLength( client, callback ) {
    // 1バイト受信を保証する
    ensureReadByte( client, 1, function(err) {
        if ( err ) { callback(err); }
        else {
            var ch1 = readByte( client );
            if( ch1 < 0x10 ) {
                callback( null, ch1 & 0x0f );
                return;
            }
            var req_byte = (ch1 & 0xf0) >> 4; 
            ensureReadByte( client, req_byte, function(err) {
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

// ファイルを送信する
//
// base_dir 基準ディレクトリ
// filelist 送信するファイル名のリスト
function writeFiles( client, base_dir, filelist, callback ) {

    var allcallback = callback;
    var filename;

    async.waterfall([
        function(callback) {
            filename = filelist.shift();
            if ( !filename ) {
                // 終了を明示するバイトを送信する
                writeByte( client, 0 );
                // 終了
                allcallback();
                return;
            }
            // 継続を表すバイトを送信する
            writeByte( client, 1 );
            // ファイル名を送信する
            writeString( client, filename );
            callback();
        },
        function(callback) {
            // ファイルを開く
            var filepath = path.join( base_dir, filename );
            console.log("send file : " + filepath );
            fs.open( filepath, "r", callback );
        },
        function(callback) {
            // ファイルを圧縮する
            zlib.gzip( buf, callback );
        },
        function(data, callback) {
            // 圧縮したバイナリを送信する
            writeBinary( client, data );
        }
    ], function(err) {
        // 自身を再帰呼び出しする
        writeFiles( client, base_dir, filelist, callback );
    });
}


// データ読み込みのCallbackを設定する
function ensureReadByte( client, length, callback ) {

    if ( client.read_buffer.length >= length ) {
        callback();
        return;
    }

    function callback_receive(chunk) {
        if ( client.read_buffer.length >= length ) {
            // 長さを越えたらもうイベントの受信をしない
            client.removeListener('data', callback_receive );
            callback();
        }
        // なお、受信時の処理自体は、client作成時に登録してある
    }
    // データ受信したときの処理
    client.on('data', callback_receive );
}

// コマンドの実行結果を受け取り保証をする
function ensureCommandResult( client, callback ) {
    ensureReadByte( client, 4, callback );
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
    // 接続番号を記録する
    client.number = global.connect_num++;
    // メールアドレスを記録する
    client.email = mailaddress;

    // データ受信用
    client.on('data', function(chunk) {
        // データの結合
        client.read_buffer = Buffer.concat( [client.read_buffer, chunk] );
     });

    async.waterfall([
        function(callback) {
            // ロック取得
            lockConn(callback);
        },
        function(callback) {
            // サーバに接続する
            client.connect(PORT, HOST, callback );
        },
        function(callback) {
             // connected
            global.connect_gqs[client.number] = client;
            ensureCommandResult(client, callback);
            writeDword( client, 0 ); // Version
        },
        function(callback) {
            // read dword
            var okcode = readDword( client );
            if (okcode != 1 ) {
                callback("Godai Quest Server接続失敗 ");
            }
            else {

                // ログインコマンドを送信
                writeDword( client, COM_TryLogon );
                writeDword( client, 0 ); // version
                var login = new LoginMessage();
                login.mail_address = mailaddress;
                // パスワードはハッシュをかけて送信する
                var passwordhash = crypto.createHash('sha512').update( password ).digest('hex');
                login.password = passwordhash;
                // 自身のバージョン番号を設定する
                login.client_version = CLIENT_VERSION;
                writeProtoMes( client, login );

                ensureCommandResult(client, callback );
            }
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                closeGodaiQuestServer(client);
                callback("ログインに失敗しました")
            }
            else {
                ensureCommandResult( client, callback );
            }
        },
        function(callback) {
            // ユーザIDを受け取る
            var userId = readDword( client );
            callback( null, userId, client );
        }
    ], function(err, userId, client ) {
        unlockConn();
        callback(err, userId, client);
    });
    // クローズ時の処理
    client.on('close', function() {
        // 削除する
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
            // ロックする
            lockConn(callback);
        },
        function(callback) {
            // ユーザ情報取得コマンドを送る
            writeDword( client, COM_GetUserInfo );
            writeDword( client, 0 ); // Version
            ensureCommandResult( client, callback );
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

            // イメージ群をURIに変換したものをセットする
            for( var it in userinfo.uesr_dic ) {
                var auser = userinfo.uesr_dic[it].auser;
                auser.uri_image = convURIImage( auser.user_image );
            }
            
            callback( null, userinfo );
        }
    ], function(err, userinfo) {
        // ロックを解除する
        unlockConn();
        callback( err, userinfo );
    });
}

// 未取得アイテム数取得
function getUnpickedupItemInfo(client, userId, dungeonId, callback) {

    var list_length = 0;
    async.waterfall( [
        function(callback) {
            // ロックをかける
            lockConn(callback);
        },
        function(callback) {
            // コマンドを送る
            writeDword( client, COM_GetUnpickedupItemInfo );
            writeDword( client, 0 ); // Version
            // 対象ユーザ
            writeDword( client, userId );
            // 対象ダンジョン
            writeDword( client, dungeonId );
            ensureCommandResult( client, callback );
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
            ensureReadByte( client, 4 * list_length, callback );
        },
        function( callback ) {
            // ItemIdを受け取る
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
            ensureCommandResult( client, callback );
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

// ユーザIdに対応するアイテム情報を得る
function getItemInfoByUserId( client, user_id, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetItemInfoByUserId );
            writeDword( client, 0 );  // version
            writeDword( client, user_id );
            ensureCommandResult( client, callback );
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
            ensureCommandResult( client, callback );
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
            ensureCommandResult( client, callback );
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
        if ( article_content === undefined )
            article_content = "";
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

            ensureCommandResult( client, callback );
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
            ensureCommandResult( client, callback );
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

// ダンジョンの深さを得る
function getDungeonDepth(clinet, dungeon_id, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetDungeonDepth );
            writeDword( client, 0 ); // version
            writeDword( client, dungeon_id );
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback( "ダンジョンの深さ取得に失敗しました");
            }
            else {
                callback();
            }
        },
        function(callback) {
            // 深さを得る
            ensureReadByte(client, 4, function(err) {
                if ( err ) {callback(err); }
                else {
                    var depth = readDword( client );
                    callback( err, depth );
                }
            });
        }
    ], function(err, depth) {
        unlockConn();
        callback(err, depth);
    });
}

// ダンジョン情報を得る
/*
message GetDungeon {
	// ダンジョンID
	optional int32	id = 1;
	// ダンジョン番号
	optional int32 	dungeon_number = 2;
}
message DungeonInfo {
	// ダンジョンの情報
	optional bytes dungeon = 1;
	// サイズX
	optional int32 size_x = 2;
	// サイズY
	optional int32 size_y = 3;
	// ダンジョン番号
	optional int32 dungeon_number = 4;
}
*/
function getDungeon( client, dungeon_id, level, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetDungeon );
            writeDword( client, 0 ); // version

            var get_dungeon = new GetDungeonMessage();
            get_dungeon.id = dungeon_id;
            get_dungeon.dungeon_number = level;
            writeProtoMes( client, get_dungeon );

            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if (okcode != 1 ) {
                callback("ダンジョン情報の読み込みに失敗しました");
            }
            else {
                callback();
            }
        },
        function(callback) {
            readProtoMes(client, callback);
        },
        function(data, callback) {
            var dungeon = DungeonInfoMessage.decode(data);
            callback(null, dungeon);
        }
    ], function(err, dungeon) {
        unlockConn();
        callback(err, dungeon);
    });
}

// ダンジョンイメージ群を得る
/*
message ImagePair {
	optional int32 number = 1;
	optional bytes image = 2;
	optional string name = 3;
	optional int32 owner = 4;
	optional sfixed64 created = 5;
	optional bool can_item_image = 6;
	optional bool new_image = 7;
}

message ImagePairDic {

	optional uint32 index = 1;
	optional ImagePair imagepair = 2;
}

message DungeonBlockImageInfo {

	optional uint32 max_image_num = 1;
	repeated ImagePairDic image_dic = 2;
}

*/
function getDungeonImageBlock(client, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetDungeonBlockImage );
            writeDword( client, 0 ); //version
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("ダンジョンイメージの取得に失敗した");
            }
            else {
                callback();
            }
        },
        function(callback) {
            readProtoMes(client, callback );
        },
        function(data, callback) {
            var dungeon_block_image_info = DungeonBlockImageInfoMessage.decode(data);
            callback( null, dungeon_block_image_info );
        }
    ], function(err, dungeon_block_image_info) {
        unlockConn();
        callback(err, dungeon_block_image_info);
    });
}

// オブジェクトの情報を得る
/*
message ObjectAttr {

	optional int32 object_id = 1;
	optional bool can_walk = 2;
	optional int32 item_id = 3;
	optional bool bNew = 4;
	optional int32 command = 5;
	optional int32 command_sub = 6;
}

message ObjectAttrDic {

	optional int32 index = 1;
	optional ObjectAttr object_attr = 2;
}

message ObjectAttrInfo {

	optional int32 new_id = 1;
	repeated ObjectAttrDic object_attr_dic = 2;
}
*/  
function getObjectAttrInfo( client, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetObjectAttrInfo );
            writeDword( client, 0 ); // version
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword(client);
            if ( okcode != 1 ) {
                callback("オブジェクトの情報取得に失敗しました");
            }
            else {
                callback();
            }
        },
        function(callback) {
            readProtoMes( client, callback );
        },
        function(data, callback) {
            var object_attr_info = ObjectAttrInfoMessage.decode(data);
            callback( null, object_attr_info );
        },
        function(object_attr_info, callback) {
            // 使いやすい用に作り替える
            var formed = []; 
            for( var it in object_attr_info.object_attr_dic ) {
                var objattr = object_attr_info.object_attr_dic[it].object_attr;
                formed[+objattr.object_id] = objattr;
            }
            callback( null, formed, object_attr_info );
        }
    ], function(err, easy_objattr_info, object_attr_info) {
        unlockConn();
        callback(err, easy_objattr_info, object_attr_info);
    });
}

// タイル情報を得る
/*
message Tile {

	optional uint64	tile_id = 1;
}

message TileDic {

	optional uint64 index = 1;
	optional Tile tile = 2;
}

message TileInfo {

	repeated TileDic tile_dic = 1;
}
*/
function getTileList( client, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetTileList );
            writeDword( client, 0 ); // version
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword(client);
            if ( okcode != 1 ) {
                callback( "タイル情報を得る" );
            }
            else {
                callback();
            }
        },
        function(callback) {
            readProtoMes( client, callback );
        },
        function(data, callback) {
            var tileinfo = TileInfoMessage.decode(data);
            callback( null, tileinfo );
        }
    ], function(err, tileinfo) {
        unlockConn();
        callback(err, tileinfo);
    });
}

// 
// ダンジョンを設定する
/*
message SetDungeon {

	// ユーザID
	optional int32 user_id = 1;
	// ダンジョン番号
	optional int32 dungeon_number = 2;

	// ダンジョンの情報
	optional DungeonInfo dungeon_info = 3;

	// イメージ情報
	optional DungeonBlockImageInfo images = 4;

	optional ObjectAttrInfo object_info = 5;

	optional TileInfo tile_info = 6;
}
message DungeonInfo {
	// ダンジョンの情報
	optional bytes dungeon = 1;
	// サイズX
	optional int32 size_x = 2;
	// サイズY
	optional int32 size_y = 3;
	// ダンジョン番号
	optional int32 dungeon_number = 4;
}
message ImagePair {
	optional int32 number = 1;
	optional bytes image = 2;
	optional string name = 3;
	optional int32 owner = 4;
	optional sfixed64 created = 5;
	optional bool can_item_image = 6;
	optional bool new_image = 7;
}

message ImagePairDic {

	optional uint32 index = 1;
	optional ImagePair imagepair = 2;
}

message DungeonBlockImageInfo {

	optional uint32 max_image_num = 1;
	repeated ImagePairDic image_dic = 2;
}

message ObjectAttr {

	optional int32 object_id = 1;
	optional bool can_walk = 2;
	optional int32 item_id = 3;
	optional bool bNew = 4;
	optional int32 command = 5;
	optional int32 command_sub = 6;
}

message ObjectAttrDic {

	optional int32 index = 1;
	optional ObjectAttr object_attr = 2;
}

message ObjectAttrInfo {

	optional int32 new_id = 1;
	repeated ObjectAttrDic object_attr_dic = 2;
}
message Tile {

	optional uint64	tile_id = 1;
}

message TileDic {

	optional uint64 index = 1;
	optional Tile tile = 2;
}

message TileInfo {

	repeated TileDic tile_dic = 1;
}

*/
function setDungeon( client, set_dungeon, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_SetDungeon );
            writeDword( client, 0 ); // version
            writeProtoMes( client, set_dungeon );
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback( "ダンジョンの設定に失敗した");
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

// SetDungeonを作成する
function makeSetDungeon() {
    var setDungeon = new SetDungeonMessage();
    return setDungeon;
}

// 大陸の土地情報を得る
/*
message IslandGround {

	optional int32 user_id = 1;
	optional int32 ix1 = 2;
	optional int32 iy1 = 3;
	optional int32 ix2 = 4;
	optional int32 iy2 = 5;
}

message IslandGroundInfo {

	repeated IslandGround ground_list = 1;
}
*/
function getIslandGroundInfo(client, callback) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_GetIslandGroundInfo );
            writeDword( client, 0 ); // version
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword(client);
            if ( okcode != 1 ) {
                callback("大陸の土地情報の取得に失敗しました");
            }
            else {
                callback();
            }
        },
        function(callback) {
            readProtoMes( client, callback );
        },
        function(data, callback) {
            var island_ground_info = IslandGroundInfoMessage.decode(data);
            callback( null, island_ground_info );
        }
    ], function(err, island_ground_info) {
        unlockConn();
        callback(err, island_ground_info);
    });

}
// 特定ユーザの大陸情報を得る
function getIslandGroundInfoByUser(client, user_id, callback) {

    async.waterfall([
        function(callback) {
            getIslandGroundInfo(client, callback);
        },
        function(island_ground_info, callback) {
            for(var it in island_ground_info.ground_list ) {
                var ground_info = island_ground_info.ground_list[it];
                if ( ground_info.user_id == user_id ) {
                    callback( null, ground_info );
                    return;
                }
            }
            callback("UserIdに対応する大陸情報がありませんでした");
        }
    ], function(err, ground_info) {
        callback(err, ground_info);
    });
}

// 適当なアイテムを得る 
/*
message ImagePair {
	optional int32 number = 1;
	optional bytes image = 2;
	optional string name = 3;
	optional int32 owner = 4;
	optional sfixed64 created = 5;
	optional bool can_item_image = 6;
	optional bool new_image = 7;
}

message ImagePairDic {

	optional uint32 index = 1;
	optional ImagePair imagepair = 2;
}
message DungeonBlockImageInfo {

	optional uint32 max_image_num = 1;
	repeated ImagePairDic image_dic = 2;
}
*/
function getSomeItemImagePair( client, index, callback ) {

    var dungeon_block_image_info;
    async.waterfall([
        function(callback) {
            getDungeonImageBlock(client, callback );
        },
        function(_dungeon_block_image_info, callback) {
            dungeon_block_image_info = _dungeon_block_image_info;

            for(var it in dungeon_block_image_info.image_dic ) {
                var imagepair = dungeon_block_image_info.image_dic[it].imagepair;
                if  ( imagepair.can_item_image ) {
                    if ( index == 0 ) {
                        callback( null, imagepair );
                        return;
                    }
                    --index;
                }
            }
            callback("アイテムが見つかりません");
        }
    ], function(err, imagepair) {
        callback(err, imagepair);
    });
}

// アイテムを設定する
function setAItem( client, aitem_mes, imagepair_mes, basedir, filelist, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_SetAItem );
            writeDword( client, 0 ); // version
            writeProtoMes( client, aitem_mes );
            writeProtoMes( client, imagepair_mes );
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("アイテムの設定に失敗しました");
            }
            else {
                callback();
            }
        },
        function(callback) {
            // ファイルを送信する
            writeFiles( client, basedir, filelist, callback );
        },
        function(callback) {
            // Itemを受信する
            readProtoMes( client, callback );
        },
        function( data, callback ) {
            var aitem = AItemMessage.decode(data);
            callback( null, aitem );
        }
    ], function(err, aitem) {
        unlockConn();
        callback(err, aitem);
    });
}

// モンスタ化する
function setMonster( client, item_id, bMonster, callback ) {

    async.waterfall([
        function(callback) {
            lockConn(callback);
        },
        function(callback) {
            writeDword( client, COM_SetMonster );
            writeDword( client, 0 ); // nVersion
            writeDword( client, item_id );
            writeDword( client, bMonster );
            ensureCommandResult( client, callback );
        },
        function(callback) {
            var okcode = readDword( client );
            if ( okcode != 1 ) {
                callback("モンスター化に失敗しました");
            }
            else {
                callback();
            }
        }
    ], function(err) {
        unlockConn();
        callback( err );
    });
}

// 新アイテムを作成する
/*
message ImagePair {
	optional int32 number = 1;
	optional bytes image = 2;
	optional string name = 3;
	optional int32 owner = 4;
	optional sfixed64 created = 5;
	optional bool can_item_image = 6;
	optional bool new_image = 7;
}
message AItem {

	optional int32 item_id  = 1;
	optional int32 item_image_id = 2;
	optional string header_string = 3;
	optional bytes header_image = 4;
	optional bool bNew = 5;
}
*/
function createNewItem( client, header_string, bMonster, moto_object_attr_info, callback ){

    var aitem;
    var object_attr_dic;
    async.waterfall([
        function(callback) {
            getSomeItemImagePair(client, 0/*index*/, callback );
        },
        function(imagepair, callback) {

//            ImagePair imagepair = new ImagePair(this.mImageID, true, this.picItem.Image, "", mGQCom.getUserID(), DateTime.Now, this.mNewItemImage);
            var new_imagepair = new ImagePairMessage();
            new_imagepair.number = imagepair.number;
            new_imagepair.image = imagepair.image;
            new_imagepair.name = imagepair.name;
            new_imagepair.owner = imagepair.owner;
            new_imagepair.created = 0;
            new_imagepair.can_item_image = true;
            new_imagepair.new_image = false;

//            AItem item = new AItem(0, this.mImageID, this.txtHeader.Text, this.picHeader.Image, true);
            var new_aitem = new AItemMessage();
            new_aitem.item_id = 0;
            new_aitem.item_image_id = imagepair.number;
            new_aitem.header_string = header_string;
            new_aitem.header_image = null;
            new_aitem.bNew = true;

//            this.mGQCom.setAItem(ref item, imagepair, this.mFileSet);
            setAItem( client, new_aitem, new_imagepair, "", [], callback );
        },
        function(_aitem, callback) {
            aitem = _aitem;

            object_attr_dic = new ObjectAttrDicMessage();
            var new_id = ++moto_object_attr_info.new_id;
            object_attr_dic.index = new_id;
            object_attr = new ObjectAttrMessage();
            object_attr_dic.object_attr = object_attr;
            object_attr.object_id = new_id;
            object_attr.can_walk = true;
            object_attr.item_id = aitem.item_id;
            object_attr.bNew = true;
            object_attr.command = COMMAND_Nothing;
            object_attr.command_sub = 0;
            moto_object_attr_info.object_attr_dic.push( object_attr_dic );
            
            // 場合によってはモンスタ化する
//            this.mGQCom.setMonster(item.getItemID(), this.chkProblem.Checked);
            //
            setMonster( client, aitem.item_id, bMonster, callback );
        }
    ], function(err) {
        unlockConn();
        callback(err, aitem);
    });
}

// バッファ内に書き込む
function writeUint32Buf( buf, index, value ) {

    buf[index+0] = value & 0xff;
    buf[index+1] = (value >> 8 ) & 0xff;
    buf[index+2] = (value >> 16 ) & 0xff;
    buf[index+3] = (value >> 24 ) & 0xff;
}


// アイテムを配置する
/*
message ObjectAttr {

	optional int32 object_id = 1;
	optional bool can_walk = 2;
	optional int32 item_id = 3;
	optional bool bNew = 4;
	optional int32 command = 5;
	optional int32 command_sub = 6;
}
未使用
*/
function placeNewItem( client, user_id, ix, iy, new_item, callback ) {

    var object_attr_info, moto_object_attr_info;
    var block_images_info;
    var tile_info;
    var dungeon_info;
    var dungeon_mes;
    
    async.waterfall([
        function(callback) {
            // オブジェクトの情報取得
            getObjectAttrInfo( client, callback );
        },
        function(_object_attr_info, _moto_object_attr_info, callback) {
            object_attr_info = _object_attr_info;
            moto_object_attr_info = _moto_object_attr_info;
            // イメージ情報
            getDungeonImageBlock(client, callback );
        },
        function(_block_images_info, callback) {
            block_images_info = _block_images_info;

            // タイル情報取得
            getTileList( client, callback );
        },
        function(_tile_info, callback ){
            tile_info = _tile_info;

            // ダンジョンを得る
            getDungeon( client, user_id, 0/*level*/, callback );
        },
        function(_dungeon_info, callback) {
            dungeon_info = _dungeon_info;
            var obj = new ObjectAttrMessage();
            obj.object_id = ++moto_object_attr_info.new_id;
            obj.can_walk = true;
            obj.item_id = new_item.item_id;
            obj.bNew = true;
            obj.command = COMMAND_Nothing;
            obj.command_sub = 0;
            var objdic = ObjectAttrDicMessage();
            objdic.index = obj.object_id;
            objdic.object_attr = obj;
            moto_object_attr_info.object_attr_dic.push(objdic);

            dungeon_mes = makeSetDungeon();
            dungeon_mes.user_id = user_id; 
            dungeon_mes.dungeon_info = dungeon_info;
            dungeon_mes.images = block_images_info;
            dungeon_mes.object_info = moto_object_attr_info;
            dungeon_mes.tile_info = tile_info;

            setDungeon( client, dungeon_mes, callback );
        },
        function(callback) {
            // オブジェクトの情報取得
            getObjectAttrInfo( client, callback );
        },
        function(_object_attr_info, _moto_object_attr_info, callback) {
            object_attr_info = _object_attr_info;
            moto_object_attr_info = _moto_object_attr_info;

            var obj;
            for(var it in object_attr_info) {
                obj = object_attr_info[it];
                if( obj.item_id == new_item.item_id )
                    break
            }
            var obj_id = obj.object_id;

            // ダンジョンの書きかえ
            var body = dungeon_info.dungeon;
            var wbody = new Uint8Array( body.array, body.offset );
            var sizex = dungeon_info.size_x;
            var index = ix*8 + iy * sizex * 8;
            writeUint32Buf( wbody, index+0, +obj_id );
            writeUint32Buf( wbody, index+4, +new_item.item_id );

            setDungeon( client, dungeon_mes, callback );
        }
    ], function(err) {
        callback(err);
    });
}

module.exports = {
    writeDword: writeDword,
    writeDwordRev: writeDwordRev,
    ensureReadByte:ensureReadByte,
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
    deleteLastItemArticle: deleteLastItemArticle,
    getDungeonDepth: getDungeonDepth,
    getDungeon:getDungeon,
    getDungeonImageBlock:getDungeonImageBlock,
    getObjectAttrInfo: getObjectAttrInfo,
    getTileList: getTileList,
    setDungeon: setDungeon,
    makeSetDungeon: makeSetDungeon,
    getIslandGroundInfo : getIslandGroundInfo,
    getIslandGroundInfoByUser : getIslandGroundInfoByUser,
    setAItem: setAItem,
    setMonster: setMonster,
    createNewItem: createNewItem,
    placeNewItem:placeNewItem,
    convURIImage: convURIImage
}
