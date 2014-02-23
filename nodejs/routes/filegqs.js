// ファイル操作関連
var async = require('async');
var fs = require('fs');
var path = require('path');

// ファイル一覧を得る
function getFileList( dir_name, callback ) {

    getFileListSub( dir_name, dir_name, [], callback );
}
// ファイル一覧を得る
function getFileListSub( dir_base, dir_read, listFiles, callback ) {

    async.waterfall([
        function(callback) {
            fs.readdir( dir_read, callback );
        },
        function(files, callback) {
            async.forEach( files, function(filename, callback) {
                var filepath = path.join( dir_read, filename);
                async.waterfall([
                    function(callback) {
                        fs.stat( filepath, callback );
                    },
                    function(stats, callback) {
                        if ( stats.isDirectory() ) {
                            getFileListSub( dir_base, filepath, listFiles, callback );
                        }
                        else {
                            var relativePath = path.join( path.relative( dir_base, dir_read), filename );
                            listFiles.push( { path:relativePath, fullpath:filepath, size:stats.size, mtime:stats.mtime} );
                            callback();
                        }
                    }
                ], function(err) {
                    callback(err);
                });
            }, function(err){
                callback(err);
            });
        }
    ], function(err) {
        callback(err, listFiles);
    });
}

module.exports = {
    getFileList:getFileList
}
