#include <Arduino.h>
#include "TimerModule.h"


TimerModule::TimerModule(std::string name, int dataPin, int clockPin, int latchPin, SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), seconds(0), serialController(serialController) {

    pinMode(dataPin, OUTPUT);
    pinMode(clockPin, OUTPUT);
    pinMode(latchPin, OUTPUT);

}

void TimerModule::enable(){

    enabled = true;
    scrambled = false;

}

void TimerModule::disable(){
    
    scrambled = false;
    enabled = false;

    //start writing to the shift registers
    digitalWrite(latchPin, LOW);
    shiftOut(dataPin, clockPin, LSBFIRST, 0b00000000);
    shiftOut(dataPin, clockPin, LSBFIRST, 0b00000000);
    //end writing to the shift registers
    digitalWrite(latchPin, HIGH);

}

void TimerModule::set(std::vector<std::string> args){

    if(args.size() == 1){

        seconds = stoi(args[0]);

        int secs = seconds % 60;
        int mins = (seconds - secs) / 60; 

        int digits[4] = {mins/10, mins % 10, secs/10, secs % 10};

        setDigits(digits, true);

        return;
    } 

    if(args.size() == 5){

        int digits[4] = {stoi(args[0]) % 11, stoi(args[1]) % 11, stoi(args[2]) % 11, stoi(args[3]) % 11};

        setDigits(digits, args[4] == "true");

        return;
    }   
}

void TimerModule::status(){
    serialController.serialWrite(name, std::to_string(seconds));
}


void TimerModule::setDigits(int* digits, bool colon){
    memcpy(digitsBuffer, digits, 4 * sizeof(int));
    colonEnabled = colon;
}

void TimerModule::scramble(){
    scrambled = true;
}

bool TimerModule::getEnabled(){
    return enabled;
}

void TimerModule::update(){

    if(!enabled) return;

    unsigned long now = micros();
    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;

    digitalWrite(latchPin, LOW);

    if(!scrambled){
        if (currentDigit < 4){  // Show Digit
            shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings[currentDigit]);
            shiftOut(dataPin, clockPin, LSBFIRST, segment_encodings[digitsBuffer[currentDigit]]);
        } else if (currentDigit == 4 && colonEnabled){  // Show Colon
            shiftOut(dataPin, clockPin, LSBFIRST, digit_encodings[4]);
            shiftOut(dataPin, clockPin, LSBFIRST, 0b00000000);
        }
        else{
            currentDigit = 0;
            digitalWrite(latchPin, HIGH);
            return;
        }
    }else{
        digitalWrite(latchPin, LOW);
        shiftOut(dataPin, clockPin, LSBFIRST, static_cast<unsigned char>(rand() % 0xff));
        shiftOut(dataPin, clockPin, LSBFIRST, static_cast<unsigned char>(rand() % 0xff));
        digitalWrite(latchPin, HIGH);
    }

    digitalWrite(latchPin, HIGH);
    currentDigit++;
}

