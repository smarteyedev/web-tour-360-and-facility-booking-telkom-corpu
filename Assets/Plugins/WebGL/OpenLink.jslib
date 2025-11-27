mergeInto(LibraryManager.library, {
    OpenInSameTab: function (url) {
        window.location.href = UTF8ToString(url);
    }
});
