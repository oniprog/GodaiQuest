/**
 * Module dependencies.
 */

var express = require('express');
var routes = require('./routes');
//var user = require('./routes/user');
var main = require('./routes/main');
var http = require('http');
var path = require('path');
//var MongoStore = require('connect-mongo')(express);
var st = require('st');

var app = express();

// all environments
app.set('port', process.env.PORT || 3000);
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'jade');
//app.use(express.session({store: new MongoStore({db:'5dai_quest_session', host:'localhost', post:3355})}));
app.use(express.favicon());
app.use(express.logger('dev'));
app.use(express.json());
app.use(express.urlencoded());
app.use(express.methodOverride());
app.use(express.cookieParser());
app.use(express.session({secret:"alkjfsdlkejk"}));
app.use(app.router);
//app.use("/public", express.static(path.join(__dirname, 'public'), {redirect:true}));


// development only
if ('development' == app.get('env')) {
  app.use(express.errorHandler());
}

// Godai Questサーバーとの接続一覧
global.connect_gqs = {};
global.connect_num = 1;

global.DOWNLOAD_FOLDER = __dirname + "/public/download/";

app.get('/', routes.index);
app.get('/login', routes.index);
app.post('/login', main.login);
//app.get('/main', main.main);
app.get('/user_list', main.user_list);
app.get('/info_list', main.info_list);
app.get('/info_list_all', main.info_list_all);
app.get('/info', main.info);
app.post('/info', main.info_post );
app.post('/info_del_article', main.info_del_article);
app.get('/info_del_article', main.info);
app.get('/logout', main.logout );
app.get('/write_info', main.write_info);
app.post('/write_info', main.write_info_post);

var mount = st({ path: __dirname + '/public', url: '/public' });

app.configure(function() {app.use(mount);});
var server = http.createServer(app);
server.listen(app.get('port'), function(){
  console.log('Express server listening on port ' + app.get('port'));
});

