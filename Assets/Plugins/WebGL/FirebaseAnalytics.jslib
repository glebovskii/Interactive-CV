mergeInto(LibraryManager.library, {
    FirebaseLogEvent: function(namePtr) {
        const name = UTF8ToString(namePtr);

        if (window.UnityAnalytics) {
            window.UnityAnalytics.log(name);
            return;
        }

        window.UnityAnalyticsQueue ??= [];
        window.UnityAnalyticsQueue.push({ name: name, parameters: {} });
    },

    FirebaseLogEventString: function(namePtr, keyPtr, valuePtr) {
        const name = UTF8ToString(namePtr);
        const key = UTF8ToString(keyPtr);
        const value = UTF8ToString(valuePtr);

        const parameters = {};
        parameters[key] = value;

        if (window.UnityAnalytics) {
            window.UnityAnalytics.log(name, parameters);
            return;
        }

        window.UnityAnalyticsQueue ??= [];
        window.UnityAnalyticsQueue.push({ name: name, parameters: parameters });
    },

    FirebaseLogEventNumber: function(namePtr, keyPtr, value) {
        const name = UTF8ToString(namePtr);
        const key = UTF8ToString(keyPtr);

        const parameters = {};
        parameters[key] = value;

        if (window.UnityAnalytics) {
            window.UnityAnalytics.log(name, parameters);
            return;
        }

        window.UnityAnalyticsQueue ??= [];
        window.UnityAnalyticsQueue.push({ name: name, parameters: parameters });
    }
});