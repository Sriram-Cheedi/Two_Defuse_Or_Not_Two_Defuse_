#include "fishingModule.h"

fishingModule::fishingModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin, SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPin(buttonPin), state(0), serialController(serialController){
    pinMode(dataPin, OUTPUT);
    pinMode(clockPin, OUTPUT);
    pinMode(latchPin, OUTPUT);
    pinMode(buttonPin,INPUT);
    randomSeed(analogRead(0));
}

void fishingModule::reset(){
    buttonState = 0;
    lastButtonState = 0;
    hookPos = 9;
    points = 0;
    randState = 0;
    state = 0;
    lastGameUpdate = 0;
    lastFishUpdate = 0;
    lastHookUpdate = 0;
    lastScoreUpdate = 0;
    lastButtonUpdate = 0;
}

void fishingModule::complete(){
    state = 2;
    points = 45;
}

void fishingModule::set(std::vector<std::string> args){
    int newState = stoi(args[0]);

    if(newState == 0){
        reset();
    }else if (newState == 2){
        complete();
    }
}

void fishingModule::status(){
    serialController.serialWrite(name, std::to_string(state));
}

void fishingModule::shift(uint32_t digit){
  digitalWrite(latchPin,LOW);
  shiftOut(dataPin,clockPin,LSBFIRST,digit);
  shiftOut(dataPin,clockPin,LSBFIRST,(digit >> 8));
  shiftOut(dataPin,clockPin,LSBFIRST,(digit >> 16));
  digitalWrite(latchPin,HIGH);
}

void fishingModule::scoreCheck(){
  int hasScored = fishLED[randState] & hookLED[hookPos];
  if (hasScored > 0){
    if(points == 0) state = 1;
    points++;
    Serial.println(points);
  }
}

void fishingModule::updateGame(){
    unsigned long micro = micros();
    unsigned long milli = millis();
    if (micro - lastGameUpdate > gameInterval && state < 2){

        lastGameUpdate = micro;
        buttonState = digitalRead(buttonPin);

        if(buttonState == HIGH && buttonState != lastButtonState){
            hookPos--;
            if(hookPos < 0) hookPos = 0;
        }

        if(milli - lastHookUpdate > hookInterval){
            hookPos++;
            if(hookPos > 9) hookPos = 9;
            lastHookUpdate = milli;
        }

        if(milli - lastScoreUpdate > scoreInterval){
            scoreCheck();
            lastScoreUpdate = milli;
        }

        if(milli - lastFishUpdate > fishInterval){
            randState = random(0,11);
            lastFishUpdate = milli;
        }

        if(points >= 45) state = 2;

        uint32_t LEDBarState = (fishLED[randState] << 14) + (hookLED[hookPos] << 4);
        shift(LEDBarState);

        lastButtonState = buttonState;
    }
}