#include "Buzzer.h"
#include "Arduino.h"

Buzzer::Buzzer(std::string name, int pin, SerialController serialController, int pitch) : 
Component(name), pin(pin), serialController(serialController), pitch(pitch) {
    pinMode(pin, OUTPUT);
}

void Buzzer::enable(){
    enabled = true;
}

void Buzzer::disable(){
    noTone(pin);
    enabled = false;
}

void Buzzer::status(){
    return;
}

void Buzzer::set(std::vector<std::string> args){
    return;
}

void Buzzer::sirenOn(){
    siren = true;
    pitch = 800;
}

void Buzzer::sirenOff(){
    siren = false;
    pitch = 50;
}

void Buzzer::update(){

    if(!enabled) return;

    if(siren){
        int now = millis();
        if(now - lastBeep < delay) return;
        if(rise){
            pitch += 50;
            if(pitch > 1500) rise = false;
        }
        else{
            pitch -= 50;
            if(pitch < 800) rise = true;
        }
        lastBeep = now;
    }

    tone(pin, pitch);


}

