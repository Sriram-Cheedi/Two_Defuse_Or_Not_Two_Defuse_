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

    disable();
}

void numberGameModule::disable(){
    
    //start writing to the shift registers
    digitalWrite(latchPin, LOW);
    if (currentDigit < 4){
        //Which digit is being displayed
        shiftOut(dataPin, clockPin, LSBFIRST, 0b11111110);
        //The number being displayed
        shiftOut(dataPin, clockPin, LSBFIRST, 0b00000000);
        currentDigit++;
    }
    else{
        currentDigit = 0;
    }
    //end writing to the shift registers
    digitalWrite(latchPin, HIGH);
    
    disabled = true;
}

void numberGameModule::reset(){

    digitsBuffer[0] = 0;
    digitsBuffer[1] = 0;
    digitsBuffer[2] = 0;
    digitsBuffer[3] = 0;

    state = 1; 
    disabled = false;
}

void numberGameModule::complete(){

    digitsBuffer[0] = secretNumber[0];
    digitsBuffer[1] = secretNumber[1];
    digitsBuffer[2] = secretNumber[2];
    digitsBuffer[3] = secretNumber[3];

    state = 2;
    disabled = false;
}

void numberGameModule::set(std::vector<std::string> args){
    if(args.size() == 1){
        int newState = stoi(args[0]);

        if(newState == 0){
            disable();
        }else if (newState == 1){
            reset();
        }else if (newState == 2){
            complete();
        }
    }else if(args.size() == 4){
        //set format: $NGM:set X X X X
        secretNumber[0] = stoi(args[0]);
        secretNumber[1] = stoi(args[1]);
        secretNumber[2] = stoi(args[2]);
        secretNumber[3] = stoi(args[3]);
        state = 0;
        digitsBuffer = {0,0,0,0};
        
        disable();
    }
}

void numberGameModule::status(){
    //$NGM:status number1 number2 number3 number4
    //$NGM:submit number1 number2 number3 number4
    //send status format: name,status,XXXX
    //NGM:status X X X X X
    std::vector<std::string> args = {std::to_string(submitting),
                                    std::to_string(digitsBuffer[0]),
                                    std::to_string(digitsBuffer[1]),
                                    std::to_string(digitsBuffer[2]),
                                    std::to_string(digitsBuffer[3])};
    serialController.serialWrite(name, args);
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
            buttonUpdate = now;
            status();
        }
    }
}

void numberGameModule::checkGuess(std::string btnName, int btnState) {
    const unsigned long debounceDelay = 500;
    unsigned long now = millis();
    if(now - submitUpdate > debounceDelay){
        if(btnState == HIGH){
            submitting = 1;
            status();
            submitting = 0;
            if(secretNumber[0] == digitsBuffer[0] && 
                secretNumber[1] == digitsBuffer[1] && 
                secretNumber[2] == digitsBuffer[2] && 
                secretNumber[3] == digitsBuffer[3]) state = 2;
            submitUpdate = now;
        }
    }
}

void numberGameModule::update(){
    //only check for game updates every 200 micro seconds
    unsigned long now = micros();

    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;

    if(disabled) return;

    for(auto& btn : buttons){
        if(state != 2){
            btn.update(); 
        }
    }
    unsigned long ledNow = millis();
    if (ledNow - ledUpdate > 300){
        ledUpdate = ledNow;
         
        ledOn = !ledOn;
    }


    //start writing to the shift registers
    digitalWrite(latchPin, LOW);
    if (currentDigit < 4){
        //Which digit is being displayed
        if(ledOn || state == 2){
            shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings_led_on[currentDigit]);
        }else{
            shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings_led_off[currentDigit]);
        }
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