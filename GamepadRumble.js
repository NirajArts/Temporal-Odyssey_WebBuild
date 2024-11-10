function RumbleGamepad(lowFrequency, highFrequency, duration) {
    var gamepad = navigator.getGamepads()[0]; // assuming the first gamepad
    if (gamepad && gamepad.vibrationActuator) {
        gamepad.vibrationActuator.playEffect("dual-rumble", {
            startDelay: 0,
            duration: duration * 1000, // Convert to milliseconds
            weakMagnitude: lowFrequency,
            strongMagnitude: highFrequency
        });
    }
}
