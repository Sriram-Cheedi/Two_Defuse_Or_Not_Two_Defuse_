#include "Button.h"
#include <Arduino.h>

Button::Button(std::string name, int pin, SerialController serialController) :
Component(name), pin(pin), serialController(serialController) {

    pinMode(pin, INPUT_PULLDOWN);

    lastState = digitalRead(pin);
}

Button::Button(std::string name, int pin, SerialController serialController, std::function<void (void)> onPressed) :
Component(name), pin(pin), serialController(serialController), onPressed(onPressed) {

    pinMode(pin, INPUT_PULLDOWN);

    lastState = digitalRead(pin);
}

void Button::status(){
    update();
    serialController.serialWrite(name, std::to_string(lastState));
}

void Button::set(std::vector<std::string> args){return;}

int Button::getState(){
    update();
    return lastState;
}

void Button::update(){

    int currentState = digitalRead(pin);

    if(currentState != lastState){
        lastState = currentState;

        if(onPressed != NULL){
            onPressed();
        }else{
            status();
        }

    }
}