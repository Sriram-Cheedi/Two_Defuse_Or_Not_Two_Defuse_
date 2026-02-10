#include "fishingModule.h"

fishingModule::fishingModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin, SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), buttonPin(buttonPin), state(0), serialController(serialController){
    pinMode(dataPin, OUTPUT);
    pinMode(clockPin, OUTPUT);
    pinMode(latchPin, OUTPUT);
    pinMode(buttonPin,INPUT);
    randomSeed(analogRead(0));
}

void fishingModule::set(std::vector<std::string> args){

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
  int hasScored = pondLED[randState] & fishLED[fishPos];
  if (hasScored > 0){
    points++;
    Serial.println(points);
  }
}

void fishingModule::updateGame(){
    unsigned long micro = micros();
    unsigned long milli = millis();
    if (micro - lastGameUpdate > gameInterval){

        lastGameUpdate = micro;
        buttonState = digitalRead(buttonPin);

        if(buttonState == HIGH && buttonState != lastButtonState){
            fishPos--;
            if(fishPos < 0) fishPos = 0;
        }

        if(milli - lastHookUpdate > hookInterval){
            fishPos++;
            if(fishPos > 9) fishPos = 9;
            lastHookUpdate = milli;
        }

        if(milli - lastFishUpdate > fishInterval){
            randState = random(0,11);
            lastFishUpdate = milli;
        }

        uint32_t LEDBarState = (pondLED[randState] << 14) + (fishLED[fishPos] << 4);
        shift(LEDBarState);

        scoreCheck();

        lastButtonState = buttonState;
    }
}