#include <Arduino.h>
#include "numberGameModule.h"

numberGameModule::numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin, int submitButtonPin, SerialController serialController, LCDModule& display)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPin(buttonPin), submitButtonPin(submitButtonPin), serialController(serialController), display(display) {
    pinMode(dataPin,OUTPUT);
    pinMode(clockPin,OUTPUT);
    pinMode(latchPin,OUTPUT);
    pinMode(buttonPin,INPUT);
    pinMode(submitButtonPin,INPUT);
    randomSeed(analogRead(A0));
}


void numberGameModule::set(std::vector<std::string> args){
    //implement methods for unity to reset the game.
    //need to learn more about how the api works before i can do that.
}

void numberGameModule::status(){

}

void numberGameModule::setupGame(){
    //set the random numbers to be guessed.
    //ensures no repeat numbers.

    secretNumber[0] = random(0,10);

    secretNumber[1] = random(0,10);
    while(true){
        if(secretNumber[1] == secretNumber[0]){
            secretNumber[1] = random(0,10);
        }
        else{
            break;
        }
    }

    secretNumber[2] = random(0,10);
    while(true){
        if(secretNumber[2] == secretNumber[1] || secretNumber[2] == secretNumber[0]){
            secretNumber[2] = random(0,10);
        }
        else{
            break;
        }
    }

    secretNumber[3] = random(0,10);
    while(true){
        if(secretNumber[3] == secretNumber[0] || secretNumber[3] == secretNumber[2] || secretNumber[3] == secretNumber[1]){
            secretNumber[3] = random(0,10);
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

    const unsigned long debounceDelay = 50;
    unsigned long now = millis();

    if(lastVoltage < 0.6){
        if(now - buttonUpdate > debounceDelay){
            if(voltage >= 2.55 && voltage <= 2.8){ //button 1
                digitsBuffer[0] = digitsBuffer[0] + 1;
                if(digitsBuffer[0] > 9){
                    digitsBuffer[0] = 0;
                }
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
            buttonUpdate = now;
        }
    }
    lastVoltage = voltage;
}

void numberGameModule::checkGuess() {
    // Reset LED states
    ledStates = 0b00000000;
    int correctPositions = 0;
    bool secretUsed[4] = {false, false, false, false};
    bool guessUsed[4] = {false, false, false, false};
    
    //Green LED
    for (int i = 0; i < 4; i++) {
        //Checks what digits are in the correct place
        if (digitsBuffer[i] == secretNumber[i]) {
            correctPositions++;
            //selects the correct led to be lit up using the ledPosition array created in the header file.
            //column 1 is for green LEDs because that matches the physical layout of the game since they are the right most LEDs.
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
                    //selects the correct led to be lit up using the ledPosition array created in the header file.
                    //column 0 is for orange LEDs because that matches the physical layout of the game since they are the left most LEDs.
                    ledStates |= (1 << ledPositions[i][0]);
                    secretUsed[j] = true;
                    guessUsed[i] = true;
                    break;
                }
            }
        }
    }
    //if all positions are correct display you win on the LCD
    if(correctPositions == 4){
        char word[] = "You Win!";
        display.LCDSet(word);
    }
    
}

void numberGameModule::updateDisplay(){
    //only check for game updates every 200 micro seconds
    unsigned long now = micros();
    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;
    buttonCheck();
    buttonState = digitalRead(submitButtonPin);
    //check the current guess when the button is pressed
    if(buttonState  ==  HIGH && buttonState  != lastButtonState){
        checkGuess();
    }

    //start writing to the shift registers
    digitalWrite(latchPin, LOW);
    if (currentDigit < 4){
        //LED display
        shiftOut(dataPin, clockPin, LSBFIRST, ledStates);
        //Which digit is being displayed
        shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings[currentDigit]);
        //The number being displayed
        shiftOut(dataPin, clockPin, LSBFIRST, segment_encodings[digitsBuffer[currentDigit]]);
        currentDigit++;
    }
    else{
        currentDigit = 0;
    }
    //end writing to the shift registers
    digitalWrite(latchPin, HIGH);
    lastButtonState = buttonState;
}