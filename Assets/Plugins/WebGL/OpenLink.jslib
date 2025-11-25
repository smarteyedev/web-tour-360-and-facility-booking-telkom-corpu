mergeInto(LibraryManager.library, {
    OpenUrlSameTab: function (urlPtr) {
        var url = UTF8ToString(urlPtr);
        window.location.href = url;
    }
});
