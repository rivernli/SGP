
function openNewWindowFromFlex(url, target) {
    if (target == null) target = '_blank';
    if (isWindowsChromeOrChromeFrame()) {
        chromeFrameOpenNewWindow(url, target);
    } else {
        window.open(url, target);
    }
}


//获取Html转义字符
function htmlEncode(html) {
    return document.createElement('encode').appendChild(
           document.createTextNode(html)).parentNode.innerHTML;
};
//获取Html 
function htmlDecode(html) {
    var a = document.createElement('decode'); a.innerHTML = html;
    return a.textContent;
};