window.voiceAssistant = {
  startRecognition: function (dotNetHelper, targetField) {

    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!SpeechRecognition) {
      alert("SpeechRecognition не підтримується у цьому браузері.");
      return;
    }

    const recognition = new SpeechRecognition();
    recognition.lang = "uk-UA";
    recognition.interimResults = false;
    recognition.maxAlternatives = 1;

    recognition.onresult = function (event) {
      const text = event.results[0][0].transcript;
      dotNetHelper.invokeMethodAsync("OnSpeechRecognized", text, targetField);
    };

    recognition.onerror = function (event) {
      console.error("Recognition error:", event.error);
      dotNetHelper.invokeMethodAsync("OnSpeechError", event.error, targetField);
    };

    recognition.onend = function () {
      // лог feedback
      console.log("Recognition ended");
    };

    recognition.start();
  },

  speak: function (text) {
    console.log("Big Button pressed");
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = "uk-UA";
    speechSynthesis.speak(utterance);
  }
};
