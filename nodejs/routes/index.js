
/*
 * GET home page.
 */

exports.index = function(req, res){
    res.render('index', { error_message: req.session.error_message });
};