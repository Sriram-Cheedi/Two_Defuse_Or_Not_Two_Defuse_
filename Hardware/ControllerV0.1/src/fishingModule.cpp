#include "fishingModule.h"

fishingModule::fishingModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin, SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPin(buttonPin), state(0), serialController(serialController),
btn("FGM1", buttonPin, serialController,[this](std::string name, int state){buttonUpdate(name, state);})
{
    pinMode(dataPin, OUTPUT);
    pinMode(clockPin, OUTPUT);
    pinMode(latchPin, OUTPUT);
    randomSeed(analogRead(A1));
}

void fishingModule::reset(){
    hookPos = 9;
    points = 0;
    randState = 0;
    state = 0;
    lastGameUpdate = 0;
    lastFishUpdate = 0;
    lastHookUpdate = 0;
    lastButtonUpdate = 0;
}

void fishingModule::complete(){
    state = 2;
    points = 45;
}

void fishingModule::set(std::vector<std::string> args){
    
}

void fishingModule::status(){
    //FGM:status 1 for correct position
    //FGM:status 2 for incorrect position
    serialController.serialWrite(name, std::to_string(state));
}

void fishingModule::shift(uint32_t digit){
  digitalWrite(latchPin,LOW);
  shiftOut(dataPin,clockPin,LSBFIRST,digit);
  shiftOut(dataPin,clockPin,LSBFIRST,(digit >> 8));
  shiftOut(dataPin,clockPin,LSBFIRST,(digit >> 16));
  digitalWrite(latchPin,HIGH);
}

void fishingModule::buttonUpdate(std::string btnName, int btnState){
    if(btnState == HIGH){
        hookPos++;
        if(hookPos > 9) hookPos = 9;
    }
}

void fishingModule::scoreCheck(){
  int hasScored = fishPos[randState] & hookLED[hookPos];
  if (hasScored > 0){
    if(state == 2){
        state = 1;
        status();
    }
    lockout = true;
  }else{
    if(state == 1){
        state = 2;
        status();
    }
    lockout = false;
  }
}

void fishingModule::update(){
    unsigned long micro = micros();
    unsigned long milli = millis();
    if (micro - lastGameUpdate > gameInterval && state < 3){
        lastGameUpdate = micro;
        if(milli - lastButtonUpdate > buttonInterval){
            btn.update();
            lastButtonUpdate = milli;
        }
        if(milli - lastHookUpdate > hookInterval && !lockout){
            hookPos--;
            if(hookPos < 0) hookPos = 0;
            lastHookUpdate = milli;
        }

        if(milli - lastFishUpdate > fishInterval){
            randState = random(0,9);
            //randState++;
            //if(randState > 11) randState = 0;
            lastFishUpdate = milli;
        }
        uint32_t LEDBarState = (fishEncoding[randState] << 14) + (hookLED[hookPos] << 4);
        shift(LEDBarState);

        scoreCheck();
    }
}