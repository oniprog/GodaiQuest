var network = require("./network");
var async = require("async"); 
//var html_encoder = require("node-html-encoder").Encoder();

// htmlエンコード(使わない。というか使えない. FAQみたらそんなの必要ないでしょって。)
function simpleHtmlEncode( str ) {

    str = str.replace("\r", "").replace("\n", "<BR>").replace(" ", "&nbsp;").replace("<", "&lt;").replace(">", "&gt;");
    return str;
}

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
exports.info_list = function(req, res) {

    var client = checkLogin(req,res);
    if ( !client )
        return;

    var user_id = req.session.user_id;
    var view_id = req.query.view_id;
    if ( !view_id )
        view_id = user_id;

    var userinfo;
    var iteminfoall, iteminfo = {};
    async.waterfall( [
        function(callback) {
            getUserInfoCache( client, req, callback );
        },
        function(_userinfo, callback) {
            userinfo = _userinfo;
            network.getItemInfo(client, callback);
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
                        iteminfo[itemid] = dic.aitem; // AItemの情報が入る
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
        res.render('info_list', {pretty:true, error_message:err, name:auser.user_name, iteminfo:iteminfo, view_id:view_id});
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
    if ( !view_id )
        view_id = user_id;

    var iteminfo = {};
    async.waterfall( [
        function(callback) {
            getUserInfoCache( client, req, callback );
        },
        function(_userinfo, callback) {
            userinfo = _userinfo;
            network.getItemInfo(client, callback);
        }, 
        function(_iteminfo, callback) {
            for(var it2 in _iteminfo.aitem_dic ) {
                var dic = _iteminfo.aitem_dic[it2];
                var itemid = dic.aitem.item_id;
//                dic.aitem.header_string = simpleHtmlEncode( dic.aitem.header_string);
                iteminfo[itemid] = dic.aitem; // AItemの情報が入る
            }
            callback( null );
        },
        function( callback ) {
            getAUser( req, view_id, callback );
        }
    ], function(err, auser) {
        res.render('info_list', {allinfo:1, error_message:err, view_id:view_id, name:auser.user_name, iteminfo:iteminfo});
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
    if ( !view_id )
        view_id = user_id;

    var iteminfo = {};

    async.waterfall( [
        function(callback) {
            getUserInfoCache( client, req, callback );
        },
        function(_userinfo, callback ) {
            userinfo = _userinfo;
            network.getItemInfo(client, callback);
        }, 
        function(_iteminfo, callback) {
            for(var it2 in _iteminfo.aitem_dic ) {
                var dic = _iteminfo.aitem_dic[it2];
                var itemid = dic.aitem.item_id;
                iteminfo[itemid] = dic.aitem; // AItemの情報が入る
            }
            callback();
        },
    ], function(err) {
        res.render('info', {error_message:err, iteminfo:iteminfo, info_id:info_id, view_id:view_id});
    });

}