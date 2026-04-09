#include "Buzzer.h"
#include "Arduino.h"

Buzzer::Buzzer(std::string name, int pin, SerialController serialController, int pitch) : 
Component(name), pin(pin), serialController(serialController), pitch(pitch) {
}

void Buzzer::enable(){
    enabled = true;
    onChange = true;
    scrambleNoise = false;
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
    onChange = true;
}

void Buzzer::sirenOff(){
    siren = false;
    pitch = 50;
    onChange = true;
}

void Buzzer::notification(){
    notificationPlay = true;
    duration = 0;
    pause = false;
    beepNum = 0;
    pitch = 550;
}

void Buzzer::scramble(){
    scrambleNoise = true;
}

void Buzzer::update(){

    if(!enabled) return;

    if(scrambleNoise){
        int now = millis();
        if(now - lastScramble > scrambleDelay){
            lastScramble = now;
            pitch = random(100, 2000);
            onChange = true;

            if(scrambleRise){
                if(scrambleDelay < 100) scrambleDelay += 5;
                else scrambleRise = false;
            }else{
                if(scrambleDelay > 10) scrambleDelay -= 5;
                else scrambleRise = true;
            }
        }
        
    }else if(siren){
        int now = millis();
        if(now - lastBeep < beepDelay) return;
        if(rise){
            pitch += 50;
            onChange = true;
            if(pitch > 1500) rise = false;
        }
        else{
            pitch -= 50;
            onChange = true;
            if(pitch < 800) rise = true;
        }
        lastBeep = now;

    }
    else if(notificationPlay){
        int now = millis();
        if (!pause) {
            if (now - lastDelay >= 200) {
                lastDelay = now;

                if (beepNum % 2 == 0) {
                    pitch = 550;
                    onChange = true;
                } else {
                    pitch = 50;
                    onChange = true;
                }

                beepNum++;

                if (beepNum >= 5) {
                    beepNum = 0;
                    pause = true;
                    pitch = 50;
                    onChange = true;
                }
            }
        } else {
            if (now - lastDelay >= 1000) {
                pause = false;
                lastDelay = now;
                duration += 1;

                if(duration > 4){
                    duration = 0;
                    notificationPlay = false;
                }
            }
        }
    }else{
        if(pitch != 50){
            onChange = true;
            pitch = 50;
        }
    }

    if(onChange){
        tone(pin,pitch);
        onChange = false;
    }
}
 
