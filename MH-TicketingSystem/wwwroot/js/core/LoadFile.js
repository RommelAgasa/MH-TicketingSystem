function load(file) {
    var src = document.createElement("script");
    src.setAttribute("type", "text/javascript");
    src.setAttribute("src", file);
    document.getElementsByTagName("head"[0].appendChild(src);
}

load("~/js/core/jquery-3.7.1.min.js");
load("~/js/core/popper.min.js");
load("~/js/core/bootstrap.min.js");
load("~/vendor/toastr/js/toastr.min.js");

