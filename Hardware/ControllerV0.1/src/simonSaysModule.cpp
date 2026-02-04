#include "SimonSaysModule.h"


SimonSaysModule::SimonSaysModule(std::string name, std::vector<int> buttonPins, SerialController serialController)
: Component(name), buttonPins(buttonPins), state(0), serialController(serialController) {

    for(size_t i = 0; i < buttonPins.size(); i++){
        Button btn = Button("SSM" + std::to_string(i), buttonPins[i], serialController, [this](std::string name, int state){buttonPressed(name, state);});   
        buttons.push_back(btn);
    }

}

void SimonSaysModule::buttonPressed(std::string btnName, int state){

    if(state == LOW){
        pressedButton = btnName;
    }

}


/* void SimonSaysModule::set_leds(int* rgb1, int* rgb2, int* rgb3, int* rgb4){

  int8_t sr1 = (rgb1[0] << 7) + (rgb1[1] << 6) + (rgb1[2] << 5)
             + (rgb2[0] << 4) + (rgb2[1] << 3) + (rgb2[2] << 2);

  int8_t sr2 = (rgb3[0] << 7) + (rgb3[1] << 6) + (rgb3[2] << 5)
             + (rgb4[0] << 4) + (rgb4[1] << 3) + (rgb4[2] << 2);          

  digitalWrite(latchPin, LOW);
  shiftOut(dataPin, clockPin, LSBFIRST, ~sr2);
  shiftOut(dataPin, clockPin, LSBFIRST, ~sr1);
  digitalWrite(latchPin, HIGH);
  
} */

void SimonSaysModule::set(std::vector<std::string> args) {
    if(args.size() == 1){
        int newState = stoi(args[0]);

        if(newState == 0){
            reset();
        }else if (newState == 2){
            //complete();
        }
    } else{

        correctSequence.clear();

        for(std::string arg : args){
            correctSequence.push_back(stoi(arg));
        }

        reset();
    }
}

void SimonSaysModule::status() {
    serialController.serialWrite(name, std::to_string(state));
}

void SimonSaysModule::reset(){

    if(threadID != -1){
        threads.kill(threadID);
    }

    state = 0;
    currentRound = 1;
    inputSequence = {};

    threadID = threads.addThread(loopTrampoline, this);
}

void SimonSaysModule::complete(){

    if(threadID != -1){
        threads.kill(threadID);
    }

    state = 0;
    currentRound = 1;
    inputSequence = {};
    state = 2;
    
    Serial.println("COMPLETE");
}

int SimonSaysModule::getInputSequence(){

    int startTime = millis(); 

    inputSequence.clear();

    while(millis() - startTime < 3000){

        for(auto& btn : buttons){
            
            btn.update();

            if(pressedButton == ""){
                continue;
            }else{

                startTime = millis();

                if(pressedButton == "SSM0"){
                inputSequence.push_back(0);
                }else
                if(pressedButton == "SSM1"){
                    inputSequence.push_back(1);
                }else 
                if(pressedButton == "SSM2"){
                    inputSequence.push_back(2);
                }else 
                if(pressedButton == "SSM3"){
                    inputSequence.push_back(3);
                }

                pressedButton = "";
            }
        }

        uint32_t waitUntil = millis() + 50;
        while (millis() < waitUntil) threads.yield();
    }

    if(inputSequence.size() == 0) return 1;

    if(inputSequence.size() != currentRound) return 0;

    for(size_t i = 0; i < currentRound; i++){
        if(inputSequence[i] != correctSequence[i]) return 0;
    }
    
    return 2;
} 

void SimonSaysModule::loop(){

    while(true){

        Serial.printf("Round %x\n", currentRound);

        if((currentRound > 1) & (state != 1)) state = 1;

        for(size_t i = 0; i < currentRound; i++){
            
            switch(correctSequence[i]) {
            case 0:

                Serial.print("1");    
                break;

            case 1:
                Serial.print("2"); 
                break;

            case 2:
                Serial.print("3"); 
                break;

            case 3:
                Serial.print("4"); 
                break;

            default:
                break;
            }

            uint32_t waitUntil = millis() + 500;
            while (millis() < waitUntil) threads.yield();
        }

        Serial.println();

        int result = getInputSequence();
        Serial.printf("Result: %d\n", result);

        if(result == 2){
            if(currentRound < correctSequence.size()){
                currentRound++;
            }else{
                complete();
                return;
            }
        }else if(result == 0){
            reset();
        }
    }

}