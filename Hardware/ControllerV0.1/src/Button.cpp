#include "Button.h"
#include <Arduino.h>


Button::Button(std::string name, int pin, SerialController serialController) :
Component(name), pin(pin), serialController(serialController) {

    pinMode(pin, INPUT_PULLDOWN);

    delay(10);

    lastState = digitalRead(pin);
}

Button::Button(std::string name, int pin, SerialController serialController, std::function<void (std::string, int)> onPressed) :
Component(name), pin(pin), serialController(serialController), onPressed(onPressed) {

    pinMode(pin, INPUT_PULLDOWN);

    delay(10);

    lastState = digitalRead(pin);
}

void Button::status(){
    update();
}

void Button::set(std::vector<std::string> args){return;}


void Button::update(){

    int currentState = digitalRead(pin);

    if(currentState != lastState){

        lastState = currentState;

        if(onPressed != NULL){
            onPressed(name, currentState);
        }else {
            serialController.serialWrite(name, std::to_string(lastState));
        }

    }
}