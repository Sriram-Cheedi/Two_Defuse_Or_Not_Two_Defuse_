#include "Button.h"
#include <Arduino.h>


Button::Button(std::string name, int pin, SerialController serialController) :
Component(name), pin(pin), serialController(serialController) {

    pinMode(pin, INPUT_PULLDOWN);

    delay(10);

}

Button::Button(std::string name, int pin, SerialController serialController, std::function<void (std::string, int)> onPressed) :
Component(name), pin(pin), serialController(serialController), onPressed(onPressed) {

    pinMode(pin, INPUT_PULLDOWN);

    delay(10);

}

void Button::status(){
    int currentState = digitalRead(pin);
    serialController.serialWrite(name, std::to_string(currentState));
}

void Button::set(std::vector<std::string> args){return;}

void Button::enable(){

    enabled = true;

}

void Button::disable(){

    enabled = false;
}

void Button::update(){

    if(!enabled) return;

    int reading = digitalRead(pin);

    if(reading != lastState){

        lastDebounceTime = millis();
    }

    if((millis() - lastDebounceTime) > debounceDelay
        && buttonState != reading){
        
        buttonState = reading;

        if(onPressed != NULL){
            onPressed(name, buttonState);
        }else {
            serialController.serialWrite(name, std::to_string(buttonState));
        }

    }

    lastState = reading;
}