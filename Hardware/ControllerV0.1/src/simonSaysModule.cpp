#include "SimonSaysModule.h"


SimonSaysModule::SimonSaysModule(std::string name, int dataPin, int clockPin, int latchPin, SerialController serialController)
: Component(name), dataPin(dataPin), clockPin(clockPin), latchPin(latchPin), state(0), serialController(serialController) {}


void SimonSaysModule::set_leds(int* rgb1, int* rgb2, int* rgb3, int* rgb4){

  int8_t sr1 = (rgb1[0] << 7) + (rgb1[1] << 6) + (rgb1[2] << 5)
             + (rgb2[0] << 4) + (rgb2[1] << 3) + (rgb2[2] << 2);

  int8_t sr2 = (rgb3[0] << 7) + (rgb3[1] << 6) + (rgb3[2] << 5)
             + (rgb4[0] << 4) + (rgb4[1] << 3) + (rgb4[2] << 2);          

  digitalWrite(latchPin, LOW);
  shiftOut(dataPin, clockPin, LSBFIRST, ~sr2);
  shiftOut(dataPin, clockPin, LSBFIRST, ~sr1);
  digitalWrite(latchPin, HIGH);
  
}

void SimonSaysModule::set(std::vector<std::string> args) {
    if(args.size() == 1){
        int newState = stoi(args[0]);

        if(newState == 0){
            reset();
        }else if (newState == 2){
            complete();
        }

    }
}

void SimonSaysModule::status() {
    serialController.serialWrite(name, std::to_string(state));
}