module.exports = function (req, res, next) {
    if (req.method === 'POST' && req.originalUrl === '/getWt') {
        console.log(req.body)
        return res.jsonp({ status: "1", tk_no: "辽A 88888", wt_num: "888.88" })
    } else if (req.method === 'POST' && req.originalUrl === '/gatePass') {
        console.log(req.body)
        return res.jsonp({ status: "1", reason: "失败原因" })
    }
  next()
}