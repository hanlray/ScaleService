function getRndInteger(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}
module.exports = function (req, res, next) {
    if (req.method === 'POST' && req.originalUrl === '/getWt') {
        //console.log(req.body)
        var r = getRndInteger(0, 1)
        var wt_num = "1888"
        if (r == 0) wt_num = "999"
        return res.jsonp({ status: "1", tk_no: "辽A 88888", wt_num: wt_num })
    } else if (req.method === 'POST' && req.originalUrl === '/gatePass') {
        //console.log(req.body)
        return res.jsonp({ status: "1", reason: "失败原因" })
    }
  next()
}