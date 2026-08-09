import { initializeApp } from "https://www.gstatic.com/firebasejs/12.16.0/firebase-app.js";
import {
    getAnalytics,
    logEvent,
    isSupported
} from "https://www.gstatic.com/firebasejs/12.16.0/firebase-analytics.js";

const firebaseConfig = {
    apiKey: "AIzaSyCWqqgTfoD0NLUbKNlRZDxSH9WYZllnjig",
    authDomain: "interactivecv-fd26b.firebaseapp.com",
    projectId: "interactivecv-fd26b",
    storageBucket: "interactivecv-fd26b.firebasestorage.app",
    messagingSenderId: "8783810585",
    appId: "1:8783810585:web:47c89dc6178bd157621a88",
    measurementId: "G-L749GV82W2"
};

window.UnityAnalyticsQueue ??= [];

let analytics = null;

window.UnityAnalytics = {
    log(name, parameters = {}) {
        console.log("[Analytics] Event:", name, parameters);

        if (analytics != null)
            logEvent(analytics, name, parameters);
        else {
            console.log("[Analytics] Firebase not ready, queueing event");
            window.UnityAnalyticsQueue.push({ name, parameters });
        }
    }
};

isSupported()
    .then(supported => {
        console.log("[Analytics] Supported:", supported);

        if (!supported) {
            console.warn("[Analytics] Firebase Analytics is not supported.");
            return;
        }

        const app = initializeApp(firebaseConfig);
        analytics = getAnalytics(app);

        console.log("[Analytics] Firebase initialized");

        for (const event of window.UnityAnalyticsQueue) {
            console.log("[Analytics] Sending queued event:", event.name);
            logEvent(analytics, event.name, event.parameters);
        }

        window.UnityAnalyticsQueue.length = 0;
    })
    .catch(error => {
        console.error("[Analytics] Initialization failed:", error);
    });