#include <Arduino.h>
#include "numberGameModule.h"

numberGameModule::numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin,SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPin(buttonPin), serialController(serialController) {
    pinMode(dataPin,OUTPUT);
    pinMode(clockPin,OUTPUT);
    pinMode(latchPin,OUTPUT);
    pinMode(buttonPin,INPUT);
    //randomNum = randomNum(0,9999);
}


void numberGameModule::set(std::vector<std::string> args){

}

void numberGameModule::status(){

}

void numberGameModule::buttonCheck(){
    //unsigned long now = micros();
    int sensorValue = analogRead(buttonPin);
    float voltage = sensorValue * (3.3 / 1023.0);
    //if (now - buttonUpdate < refreshInterval*100) return;
    //buttonUpdate = now;
    if(lastButtonState > 3.1){
        if(voltage <= 1.66){
            digitsBuffer[0] = digitsBuffer[0] + 1;
            if(digitsBuffer[0] > 9){
                digitsBuffer[0] = 0;
            }
        }
        else if(voltage <= 2.28){
            digitsBuffer[1] = digitsBuffer[1] + 1;
            if(digitsBuffer[1] > 9){
                digitsBuffer[1] = 0;
            }
        }
        else if(voltage <= 2.72){
            digitsBuffer[2] = digitsBuffer[2] + 1;
            if(digitsBuffer[2] > 9){
                digitsBuffer[2] = 0;
            }
        }
        else if(voltage <= 3){
            digitsBuffer[3] = digitsBuffer[3] + 1;
            if(digitsBuffer[3] > 9){
                digitsBuffer[3] = 0;
            }
        }
        Serial.println(voltage);
    }
    

    lastButtonState = voltage;
    /*buttonState = digitalRead(buttonPin);

    if (buttonState != lastButtonState) {
        if (buttonState == HIGH) {
            buttonPushCounter++;
            if(buttonPushCounter > 9999){
                buttonPushCounter = 0;
            }
            digitsBuffer[0] = buttonPushCounter / 1000;
            digitsBuffer[1] = (buttonPushCounter % 1000) / 100;
            digitsBuffer[2] = (buttonPushCounter % 100) / 10;
            digitsBuffer[3] = (buttonPushCounter % 10);
        }
    }
    lastButtonState = buttonState;*/
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