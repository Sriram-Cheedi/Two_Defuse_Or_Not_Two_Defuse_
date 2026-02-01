#include <Arduino.h>
#include "timerModule.h"


timerModule::timerModule(std::string name, int dataPin, int clockPin, int latchPin, SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), seconds(0), serialController(serialController) {}


void timerModule::set(std::vector<std::string> args){
    if(args.size() == 1){
        int secondsLeft = stoi(args[0]);

        int seconds = secondsLeft % 60;
        int minutes = (secondsLeft - seconds) / 60; 

        int digits[4] = {minutes/10, minutes % 10, seconds/10, seconds % 10};

        setDigits(digits, true);
    } else if(args.size() == 5){

        int digits[4] = {stoi(args[0]) % 10, stoi(args[1]) % 10, stoi(args[2]) % 10, stoi(args[3]) % 10};

        setDigits(digits, args[4] == "true");
    }   
}

void timerModule::status(){
    serialController.serialWrite(name, std::to_string(seconds));
}


void timerModule::setDigits(int* digits, bool colon){
    memccpy(digitsBuffer, digits, 4, sizeof(int));
    colonEnabled = colon;
}

void timerModule::updateDisplay(){

    unsigned long now = micros();
    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;

    digitalWrite(latchPin, LOW);

    if (currentDigit < 4){  // Show Digit
        shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings[currentDigit]);
        shiftOut(dataPin, clockPin, LSBFIRST, segment_encodings[digitsBuffer[currentDigit]]);
    }
    
    if (currentDigit == 4 && colonEnabled){  // Show Colon
        shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings[4]);
        shiftOut(dataPin, clockPin, LSBFIRST, 0b00000000);
    }
    else{
        currentDigit = 0;
    }

    digitalWrite(latchPin, HIGH);
    currentDigit++;
}

