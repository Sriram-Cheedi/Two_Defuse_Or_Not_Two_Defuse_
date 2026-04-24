#include "Buzzer.h"
#include "Arduino.h"
#include "TeensyThreads.h"
#include "pitches.h"

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
    if(args.size() > 0){
        if(std::stoi(args[0]) == 1){
            siren = false;
            notificationPlay = false;
            scrambleNoise = false;
            pause = false;
        }
    }
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


void Buzzer::win(){
    
    int noteC5 = 523;
    int noteE5 = 659;
    int noteG5 = 784;
    int noteC6 = 1047;

    tone(pin, noteC5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    tone(pin, noteE5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    tone(pin, noteG5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    tone(pin, noteC6);
    threads.delay(200); 
    noTone(pin);
    threads.delay(100); 

    tone(pin, noteG5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    tone(pin, noteC6);
    threads.delay(400); 
    noTone(pin);

    onChange = true;
}

void Buzzer::lose(){
    int noteEb4 = 311; 
    int noteDb4 = 277; 
    int noteC4  = 261; 
    int noteB2  = 123; 

    tone(pin, noteEb4);
    threads.delay(150);
    noTone(pin);
    threads.delay(50);

    tone(pin, noteDb4);
    threads.delay(150);
    noTone(pin);
    threads.delay(50);

    tone(pin, noteC4);
    threads.delay(150);
    noTone(pin);
    threads.delay(50);

    tone(pin, noteB2);
    threads.delay(600); 
    noTone(pin);

    onChange = true;
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

#define NOTE_COUNT 35 


const int melody[NOTE_COUNT] = {
  NOTE_G4, NOTE_A4, NOTE_C5, NOTE_A4, NOTE_E5, REST, NOTE_E5, REST, NOTE_D5, REST,
  NOTE_G4, NOTE_A4, NOTE_C5, NOTE_A4, NOTE_D5, REST, NOTE_D5, REST, NOTE_C5, NOTE_B4, NOTE_A4, REST,
  NOTE_G4, NOTE_A4, NOTE_C5, NOTE_A4, NOTE_C5, NOTE_D5, NOTE_B4, NOTE_A4, NOTE_G4, REST,
  NOTE_G4, NOTE_D5, NOTE_C5
};


const float timing[NOTE_COUNT] = {
  2, 2, 2, 2, 3, 1, 3, 1, 6, 2, 
  2, 2, 2, 2, 3, 1, 3, 1, 2, 2, 4, 2,
  2, 2, 2, 2, 4, 2, 2, 2, 4, 2,
  4, 4, 8
};

void Buzzer::rickRoll() {
  const float tempo = 200.0;
  const uint32_t sixteenthNoteUs = (60000000UL / tempo) / 4;

  for (int i = 0; i < NOTE_COUNT; i++) {
    uint32_t noteDurationUs = timing[i] * sixteenthNoteUs;
    uint32_t startTime = micros();

    if (melody[i] != REST) {
      tone(pin, melody[i]);
      uint32_t playTimeUs = noteDurationUs * 0.80;
      while (micros() - startTime < playTimeUs);
      noTone(pin);
    }
    
    while (micros() - startTime < noteDurationUs);
  }

  onChange = true;
}