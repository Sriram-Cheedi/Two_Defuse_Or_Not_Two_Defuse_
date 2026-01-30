#include <Arduino.h>
#include "numberGameModule.h"

numberGameModule::numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin,SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPin(buttonPin), serialController(serialController) {
    pinMode(dataPin,OUTPUT);
    pinMode(clockPin,OUTPUT);
    pinMode(latchPin,OUTPUT);
    pinMode(buttonPin,INPUT);
}


void numberGameModule::set(std::vector<std::string> args){

}

void numberGameModule::status(){

}

void numberGameModule::buttonCheck(){
    buttonState = digitalRead(buttonPin);

    if (buttonState != lastButtonState) {
        if (buttonState == HIGH) {
            buttonPushCounter++;
            if(buttonPushCounter > 9999){
                buttonPushCounter = 0;
            }
            digitsBuffer[0] = buttonPushCounter / 1000;
            Serial.println(digitsBuffer[0]);
            digitsBuffer[1] = (buttonPushCounter % 1000) / 100;
            Serial.println(digitsBuffer[1]);
            digitsBuffer[2] = (buttonPushCounter % 100) / 10;
            Serial.println(digitsBuffer[2]);
            digitsBuffer[3] = (buttonPushCounter % 10);
            Serial.println(digitsBuffer[3]);
        }
    }
    lastButtonState = buttonState;
}

void numberGameModule::updateDisplay(){
    unsigned long now = micros();
    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;
    buttonCheck();

    digitalWrite(latchPin, LOW);

    if (currentDigit < 4){  // Show Digit
        shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings[currentDigit]);
        shiftOut(dataPin, clockPin, LSBFIRST, segment_encodings[digitsBuffer[currentDigit]]);
        currentDigit++;
    }
    else{
        currentDigit = 0;
    }

    digitalWrite(latchPin, HIGH);
}