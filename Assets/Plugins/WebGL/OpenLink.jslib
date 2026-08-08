mergeInto(LibraryManager.library, {
    PrepareLinkWindow: function() {
        window.unityLinkWindow = window.open("about:blank", "_blank");
    },

    OpenPreparedLink: function(urlPtr) {
        const url = UTF8ToString(urlPtr);

        if (window.unityLinkWindow && !window.unityLinkWindow.closed) {
            window.unityLinkWindow.location.href = url;
            window.unityLinkWindow = null;
        }
    }
});