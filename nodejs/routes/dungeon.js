/////////////////////////////////////////////////////////
// ダンジョンの処理
/////////////////////////////////////////////////////////
var async = require('async');

var COMMAND_Nothing = 0;
var COMMAND_GoUp = 1;
var COMMAND_GoDown = 2;
var COMMAND_IntoDungeon = 3;
var COMMAND_GoOutDungeon = 4;

//
function isItem( num ) {
    return num != 0 && num != null;
}
function getObjectIdFromTile( num ) {
    return +num & 0xffffffff;
}
function getItemIdFromTile( num ) {
    return +num >> 16;
}

// tileInfoからObjId -> ItemId(ImageId)を作成する
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
function makeMapObjIdToImageIdFromTileInfo( tileinfo ) {

    var ret = [];
    for(var it in tileinfo.tile_dic){
        var tile = tileinfo.tile_dic[it].tile.tile_id;
        if (tile == null ) {
            ret[0] = 0;
        }
        else{
            var objId = tile.getLowBitsUnsigned();
            var itemId = tile.getHighBitsUnsigned();
            ret[objId] = itemId;
        }
    }
    return ret;
}

// ダンジョンの空きスペースの数を数える
/*
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
function getDungeonSpaceCnt(dungeon_info, objattr_info) {

    var cnt = 0;
    var body = dungeon_info.dungeon;
    body = new Int32Array( body.toArrayBuffer() );
    for(var it=0; it<body.length; it+=2) {
        var objId = body[it+0]; 
        var itemId = body[it+1]; 
        if ( !isItem( objattr_info[objId].item_id ) ) {
            ++cnt;
        }
    }
    return cnt;
}

function writeUint32( buf, index, value ) {

    buf[index+0] = value & 0xff;
    buf[index+1] = (value >> 8 ) & 0xff;
    buf[index+2] = (value >> 16 ) & 0xff;
    buf[index+3] = (value >> 24 ) & 0xff;
}

// 大陸に入り口を置く
function setDungeonEntracne( island_info, island_ground_info, objattr_info, mapObjIdToImageId) {
    var ix1 = +island_ground_info.ix1;
    var iy1 = +island_ground_info.iy1;
    var ix2 = +island_ground_info.ix2;
    var iy2 = +island_ground_info.iy2;

    var body = island_info.dungeon;
    //for(var t1 in body) {
     //   console.log(t1);
    //}
    body = new Int32Array( body.toArrayBuffer() );
    var sizex = +island_info.size_x * 2;
    for( var ix=ix1; ix<=ix2; ++ix ) {
        for( var iy=iy1; iy<=iy2; ++iy ) {
            var objId = body[ix*2+iy*sizex + 0 ];
            if ( objattr_info[objId].command == COMMAND_IntoDungeon ) {
                return; // 入り口があれば文句はない
            }
        }
    }

    // 入り口のオブジェクトを得る
    var objattr;
    for(var it in objattr_info) {
        objattr = objattr_info[it];
        if ( objattr.command == COMMAND_IntoDungeon )
            break;
    }
    if ( !objattr ) return;
    // objattr.item_idは頼りにならない
    var image_id = mapObjIdToImageId[objattr.object_id];
        
    // 強制書き込み実行
    var iforcex = Math.floor( Math.random() * (ix2-ix1)) + ix1;
    var iforcey = Math.floor( Math.random() * (iy2-iy1)) + iy1;

    var offset = island_info.dungeon.offset;
    var buf = new Uint8Array( island_info.dungeon.array, offset );

    var address = iforcex * 8 + iforcey * sizex * 8/2;
    writeUint32(buf, address+0, +objattr.object_id );
    writeUint32(buf, address+4, +image_id );
}

// ダンジョン内に情報を置く
function setPlaceNewInfo( dungeon_info, objattr_info, mapObjIdToItemId ) {
    var body = dungeon_info.dungeon;
    var rbody = new Int32Array( body.toArrayBuffer() );
    var wbody = new Uint8Array( body.array, body.offset );

    // 空きスペースを見つける
    var sizex = dungeon.size_x;
    var sizey = dungeon.size_y;
    var it, size = sizex * sizeY * 2;
    for( it=0; it<size; it+=2 ) {
        var objId = rbody[it+0];
        var obj = objattr_info[objId];
        if ( obj.command == COMMAND_GoOutDungeon )
            continue;
        if ( !isItem(obj.item_id) )
            break; // アイテム以外の場所
    }
    if ( it == size )
        return false;

    

    return true;
}

module.exports = {
    getDungeonSpaceCnt: getDungeonSpaceCnt,
    setDungeonEntracne: setDungeonEntracne,
    makeMapObjIdToImageIdFromTileInfo: makeMapObjIdToImageIdFromTileInfo,
    setPlaceNewInfo: setPlaceNewInfo
};
