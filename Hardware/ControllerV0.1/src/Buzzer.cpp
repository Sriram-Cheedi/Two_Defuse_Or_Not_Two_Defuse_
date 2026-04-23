#include "Buzzer.h"
#include "Arduino.h"
#include "TeensyThreads.h"

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

#define NOTE_C4  262
#define NOTE_CS4 277
#define NOTE_D4  294
#define NOTE_DS4 311
#define NOTE_E4  330
#define NOTE_F4  349
#define NOTE_FS4 370
#define NOTE_G4  392
#define NOTE_GS4 415
#define NOTE_A4  440
#define NOTE_AS4 466
#define NOTE_B4  494

#define NOTE_C5  523
#define NOTE_CS5 554
#define NOTE_D5  587
#define NOTE_DS5 622
#define NOTE_E5  659
#define NOTE_F5  698
#define NOTE_FS5 740
#define NOTE_G5  784
#define NOTE_GS5 831
#define NOTE_A5  880
#define NOTE_AS5 932
#define NOTE_B5  988

#define NOTE_C6  1047
#define NOTE_CS6 1109
#define NOTE_D6  1175
#define NOTE_DS6 1245
#define NOTE_E6  1319
#define NOTE_F6  1397
#define NOTE_FS6 1480
#define NOTE_G6  1568
#define NOTE_GS6 1661
#define NOTE_A6  1760
#define NOTE_AS6 1865
#define NOTE_B6  1976

#define NOTE_C7  2093

void Buzzer::win(){
    
    // Note Frequencies
    int noteC5 = 523;
    int noteE5 = 659;
    int noteG5 = 784;
    int noteC6 = 1047;

    // Note 1: Short & Punchy
    tone(pin, noteC5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    // Note 2: Short & Punchy
    tone(pin, noteE5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    // Note 3: The "Jump"
    tone(pin, noteG5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    // Final Note: Long and Triumphantly High
    tone(pin, noteC6);
    threads.delay(200); 
    noTone(pin);
    threads.delay(100); 

    // Note 3: The "Jump"
    tone(pin, noteG5);
    threads.delay(100);
    noTone(pin);
    threads.delay(50);

    // Final Note: Long and Triumphantly High
    tone(pin, noteC6);
    threads.delay(400); 
    noTone(pin);

}

void Buzzer::lose(){
    // Note Frequencies (Lower and more somber/dissonant)
    int noteEb4 = 311; // Minor tone
    int noteDb4 = 277; // Lower
    int noteC4  = 261; // Root
    int noteB2  = 123; // Deep, "grumbly" low note

    // Note 1: Sudden realization (Minor 3rd)
    tone(pin, noteEb4);
    threads.delay(150);
    noTone(pin);
    threads.delay(50);

    // Note 2: Dropping down
    tone(pin, noteDb4);
    threads.delay(150);
    noTone(pin);
    threads.delay(50);

    // Note 3: Dropping further
    tone(pin, noteC4);
    threads.delay(150);
    noTone(pin);
    threads.delay(50);

    // Final Note: The "Thud" (Low, long, and vibrating)
    tone(pin, noteB2);
    threads.delay(600); 
    noTone(pin);
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

// Frequencies
#define NOTE_G4  392
#define NOTE_A4  440
#define NOTE_B4  494
#define NOTE_C5  523
#define NOTE_D5  587
#define NOTE_E5  659
#define REST     0

#define NOTE_COUNT 35 // Increased for better phrasing

// Frequencies (C-Major)
#define NOTE_G4  392
#define NOTE_A4  440
#define NOTE_C5  523
#define NOTE_D5  587
#define NOTE_E5  659
#define NOTE_B4  494
#define REST     0

const int melody[NOTE_COUNT] = {
  // Never gonna give you up
  NOTE_G4, NOTE_A4, NOTE_C5, NOTE_A4, NOTE_E5, REST, NOTE_E5, REST, NOTE_D5, REST,
  // Never gonna let you down
  NOTE_G4, NOTE_A4, NOTE_C5, NOTE_A4, NOTE_D5, REST, NOTE_D5, REST, NOTE_C5, NOTE_B4, NOTE_A4, REST,
  // Never gonna run around...
  NOTE_G4, NOTE_A4, NOTE_C5, NOTE_A4, NOTE_C5, NOTE_D5, NOTE_B4, NOTE_A4, NOTE_G4, REST,
  // and desert you
  NOTE_G4, NOTE_D5, NOTE_C5
};

// Precise 16th-note units. 
// 2 = 8th note, 3 = dotted 8th, 4 = quarter, etc.
const float timing[NOTE_COUNT] = {
  // "Nev-er gon-na give you up"
  2, 2, 2, 2, 3, 1, 3, 1, 6, 2, 
  // "Nev-er gon-na let you down"
  2, 2, 2, 2, 3, 1, 3, 1, 2, 2, 4, 2,
  // "Nev-er gon-na run a-round"
  2, 2, 2, 2, 4, 2, 2, 2, 4, 2,
  // "and des-ert you"
  4, 4, 8
};

void Buzzer::rickRoll() {
  const float tempo = 200.0;
  // Calculate 16th note in microseconds
  const uint32_t sixteenthNoteUs = (60000000UL / tempo) / 4;

  for (int i = 0; i < NOTE_COUNT; i++) {
    uint32_t noteDurationUs = timing[i] * sixteenthNoteUs;
    uint32_t startTime = micros();

    if (melody[i] != REST) {
      tone(pin, melody[i]);
      // Play for 80% of the duration to keep notes "staccato" and punchy
      uint32_t playTimeUs = noteDurationUs * 0.80;
      while (micros() - startTime < playTimeUs);
      noTone(pin);
    }
    
    // Wait for the full rhythmic slot to finish
    while (micros() - startTime < noteDurationUs);
  }
}