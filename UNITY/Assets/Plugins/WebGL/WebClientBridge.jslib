mergeInto(LibraryManager.library, {
  WOT_RequestWebClientConfig: function () {
    window.parent.postMessage(
      { type: 'wot:unity-ready' },
      window.location.origin
    );
  },
});
