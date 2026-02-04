#include <Arduino.h>
#include "numberGameModule.h"

numberGameModule::numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin,SerialController serialController, LCDModule& display)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPin(buttonPin), serialController(serialController),display(display) {
    pinMode(dataPin,OUTPUT);
    pinMode(clockPin,OUTPUT);
    pinMode(latchPin,OUTPUT);
    pinMode(buttonPin,INPUT);
    pinMode(37,INPUT);
    randomSeed(analogRead(A0));
}


void numberGameModule::set(std::vector<std::string> args){

}

void numberGameModule::status(){

}

void numberGameModule::setupGame(){
    //set the random numbers to be guessed.
    //ensures no repeat numbers.

    secretNumber[0] = random(0,9);

    secretNumber[1] = random(0,9);
    while(true){
        if(secretNumber[1] == secretNumber[0]){
            secretNumber[1] = random(0,9);
        }
        else{
            break;
        }
    }

    secretNumber[2] = random(0,9);
    while(true){
        if(secretNumber[2] == secretNumber[1] || secretNumber[2] == secretNumber[0]){
            secretNumber[2] = random(0,9);
        }
        else{
            break;
        }
    }

    secretNumber[3] = random(0,9);
    while(true){
        if(secretNumber[3] == secretNumber[0] || secretNumber[3] == secretNumber[2] || secretNumber[3] == secretNumber[1]){
            secretNumber[3] = random(0,9);
        }
        else{
            break;
        }
    }
}

void numberGameModule::buttonCheck(){
    //Resitor ladder DAC buttons
    //Each button has a different threshold voltage.
    int sensorValue = analogRead(buttonPin);
    
    float voltage = sensorValue * (3.3 / 1023.0);
    if(lastVoltage < 0.6){
        if(voltage >= 2.55 && voltage <= 2.8){ //button 1
            digitsBuffer[0] = digitsBuffer[0] + 1;
            if(digitsBuffer[0] > 9){
                digitsBuffer[0] = 0;
            }
            Serial.print(secretNumber[0]);
            Serial.print(secretNumber[1]);
            Serial.print(secretNumber[2]);
            Serial.println(secretNumber[3]);
        }
        else if(voltage >= 2.3 && voltage <= 2.5){ //button 2
            digitsBuffer[1] = digitsBuffer[1] + 1;
            if(digitsBuffer[1] > 9){
                digitsBuffer[1] = 0;
            }
        }
        else if(voltage >= 1.9 && voltage <= 2.2){ //button 3
            digitsBuffer[2] = digitsBuffer[2] + 1;
            if(digitsBuffer[2] > 9){
                digitsBuffer[2] = 0;
            }
        }
        else if(voltage >= 1.1 && voltage <= 1.4){ //button 4
            digitsBuffer[3] = digitsBuffer[3] + 1;
            if(digitsBuffer[3] > 9){
                digitsBuffer[3] = 0;
            }
        }
        //Serial.println(voltage);
    }

    lastVoltage = voltage;
}

void numberGameModule::checkGuess() {
    // Reset LED states
    ledStates = 0b00000000;
    int correctPositions = 0;
    int correctDigits = 0;
    bool secretUsed[4] = {false, false, false, false};
    bool guessUsed[4] = {false, false, false, false};
    
    //Green LED
    for (int i = 0; i < 4; i++) {
        //Checks what digits are in the correct place
        if (digitsBuffer[i] == secretNumber[i]) {
            correctPositions++;
            ledStates |= (1 << ledPositions[i][1]);
            secretUsed[i] = true;
            guessUsed[i] = true;
        }
    }
    //Orange LED
    for (int i = 0; i < 4; i++) {
        //Checks what digits are correct but in the wrong position.
        if (!guessUsed[i]) {
            for (int j = 0; j < 4; j++) {
                if (!secretUsed[j] && digitsBuffer[i] == secretNumber[j]) {
                    correctDigits++;
                    ledStates |= (1 << ledPositions[i][0]);
                    secretUsed[j] = true;
                    guessUsed[i] = true;
                    break;
                }
            }
        }
    }
    attempts++;
    if(correctPositions == 4){
        char word[] = "You Win! ";
        display.LCDSet(word);
    }
    
}

void numberGameModule::updateDisplay(){
    unsigned long now = micros();
    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;
    buttonCheck();
    int state = digitalRead(37);
    if(state ==  HIGH && state != lastButtonState){
        checkGuess();
    }
    digitalWrite(latchPin, LOW);

    if (currentDigit < 4){  // Show Digit
        shiftOut(dataPin, clockPin, LSBFIRST, ledStates);
        shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings[currentDigit]);
        shiftOut(dataPin, clockPin, LSBFIRST, segment_encodings[digitsBuffer[currentDigit]]);
        currentDigit++;
    }
    else{
        currentDigit = 0;
    }

    digitalWrite(latchPin, HIGH);
    lastButtonState = state;
}