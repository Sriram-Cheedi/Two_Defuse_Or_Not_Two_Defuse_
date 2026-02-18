#include <Arduino.h>
#include "numberGameModule.h"

numberGameModule::numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, std::array<int,5> buttonPins, SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPins(buttonPins), serialController(serialController) {
    pinMode(dataPin,OUTPUT);
    pinMode(clockPin,OUTPUT);
    pinMode(latchPin,OUTPUT);
    for(size_t i = 0; i < 4; i++){
        Button btn = Button("NGM" + std::to_string(i), buttonPins[i], serialController, [this](std::string name, int state){buttonCheck(name, state);});   
        buttons.push_back(btn);
    }
    Button btn = Button("NGM4", buttonPins[4], serialController, [this](std::string name, int state){checkGuess(name, state);});   
    buttons.push_back(btn);
    randomSeed(analogRead(A0));
    setupGame();
}


void numberGameModule::set(std::vector<std::string> args){
    //implement methods for unity to reset the game.
    //need to learn more about how the api works before i can do that.
    //int num = stoi(args[0]);
    
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

void numberGameModule::buttonCheck(std::string btnName, int btnState){
    const unsigned long debounceDelay = 50;
    unsigned long now = millis();
    int index = stoi(btnName.substr(3));
    if(now - buttonUpdate > debounceDelay){
        if(btnState == HIGH){
            digitsBuffer[index] += 1;
            if(digitsBuffer[index] > 9){
                digitsBuffer[index] = 0;
            }
        }
        buttonUpdate = now;
    }
}

void numberGameModule::checkGuess(std::string btnName, int btnState) {
    if(btnState == HIGH){
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
        if(correctPositions == 4){
            state = 2;
        }
    }
}

void numberGameModule::update(){
    //only check for game updates every 200 micro seconds
    unsigned long now = micros();
    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;
    for(auto& btn : buttons){
        if(state != 2){
            btn.update(); 
        }
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
}