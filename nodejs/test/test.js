var assert = require('assert');
var async = require('async');
var chai = require('chai');
var assert = chai.assert;
var network =require('../routes/network');

function DummyClient() {
    this._buffer = new Buffer(0);
}

DummyClient.prototype = {
    write: function(buf, callback) {
        this._buffer = Buffer.concat( [this._buffer, buf] );
        if ( callback )
            callback();
    },
    get: function(index) {
        return this._buffer.readUInt8(index);
    },
    getDword: function(index) {
        return this._buffer.readUInt32BE(index);
    },
    getDwordRev: function(index) {
        return this._buffer.readUInt32LE(index);
    }
};

describe("network", function() {
    describe("writeDword())", function() {
        it("test1", function() {
            var dc = new DummyClient();
            network.writeDword( dc, 1234 );
            assert.equal( 1234, dc.getDword(0) );
        });
        it("test callback interface", function(done) {
            var dc = new DummyClient();
            network.writeDword( dc, 1234, function() {
                assert.equal( 1234, dc.getDword(0) );
                done();
            });
        });
    });
    describe("lockConn(), unlockConn()", function() {
        it("test1", function(done) {
            var cnt = 0;
            network.lockConn(function() {
                assert.equal(++cnt, 1);
                network.lockConn(function() {
                    assert.equal(++cnt, 2);
                    network.unlockConn();
                });
                network.unlockConn();
                assert.equal(++cnt, 3);
                done();
            });
        });
        it("test2", function(done) {
            var cnt = 0;
            network.lockConn(function() {
                assert.equal(++cnt, 1);
                network.lockConn(function() {
                    assert.equal(++cnt, 3);
                    network.lockConn(function() {
                        assert.equal(++cnt, 5);
                        network.unlockConn();
                        assert.equal(++cnt, 6);
                    });
                    assert.equal(++cnt, 4);
                    network.unlockConn();
                    assert.equal(++cnt, 7);
                });
                assert.equal(++cnt, 2);
                network.unlockConn();
                assert.equal(++cnt, 8);
                done();
            });
        });
    });
});
                       