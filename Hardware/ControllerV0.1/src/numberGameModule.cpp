#include <Arduino.h>
#include "numberGameModule.h"

numberGameModule::numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, std::array<int,5> buttonPins, SerialController serialController, LCDModule& display)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPins(buttonPins), serialController(serialController), display(display) {
    pinMode(dataPin,OUTPUT);
    pinMode(clockPin,OUTPUT);
    pinMode(latchPin,OUTPUT);
    for(int i = 0; i < 5; i++){
        pinMode(buttonPins[i],INPUT_PULLDOWN);
    }
    randomSeed(analogRead(A0));
}


void numberGameModule::set(std::vector<std::string> args){
    //implement methods for unity to reset the game.
    //need to learn more about how the api works before i can do that.
    int num = stoi(args[0]);
    
}

void numberGameModule::status(){
    serialController.serialWrite(name, std::to_string(state));
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
    const unsigned long debounceDelay = 50;
    unsigned long now = millis();
    int state0 = digitalRead(buttonPins[0]);
    int state1 = digitalRead(buttonPins[1]);
    int state2 = digitalRead(buttonPins[2]);
    int state3 = digitalRead(buttonPins[3]);
    if(now - buttonUpdate > debounceDelay){
        if(state0 = HIGH && lastInputStates[0] != state0){
            digitsBuffer[0] += 1;
            if(digitsBuffer[0] > 9){
                digitsBuffer[0] = 0;
            }
        }
        if(state1 = HIGH && lastInputStates[1] != state1){
            digitsBuffer[1] += 1;
            if(digitsBuffer[1] > 9){
                digitsBuffer[1] = 0;
            }
        }
        if(state2 = HIGH && lastInputStates[2] != state2){
            digitsBuffer[2] += 1;
            if(digitsBuffer[2] > 9){
                digitsBuffer[2] = 0;
            }
        }
        if(state3 = HIGH && lastInputStates[3] != state3){
            digitsBuffer[3] += 1;
            if(digitsBuffer[3] > 9){
                digitsBuffer[3] = 0;
            }
        }
        buttonUpdate = now;  
    }
    
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
    buttonState = digitalRead(buttonPins[4]);
    //check the current guess when the button is pressed
    if(buttonState  ==  HIGH && buttonState  != lastInputStates[4]){
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
    lastInputStates[4] = buttonState;
}