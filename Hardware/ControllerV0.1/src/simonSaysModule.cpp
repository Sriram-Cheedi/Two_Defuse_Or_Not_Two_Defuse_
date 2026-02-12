#include "SimonSaysModule.h"


SimonSaysModule::SimonSaysModule(std::string name, std::vector<int> buttonPins, int rgbPin, SerialController serialController)
: Component(name), buttonPins(buttonPins), rgbPin(rgbPin), state(0), serialController(serialController) {

    for(size_t i = 0; i < buttonPins.size(); i++){
        Button btn = Button("SSM" + std::to_string(i), buttonPins[i], serialController, [this](std::string name, int state){buttonPressed(name, state);});   
        buttons.push_back(btn);
    }
}


void SimonSaysModule::init(){
    strip.begin();

    setPixelBuffer({strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0)});
}

void SimonSaysModule::update(){

    if(pixelBufferMutex.try_lock()){
        for(int i = 0; i < 4; i++){
            strip.setPixel(i, pixelBuffer[i]);
        }
        strip.show();
        pixelBufferMutex.unlock();
    }
}

void SimonSaysModule::buttonPressed(std::string btnName, int state){
    
    int index = stoi(btnName.substr(3));

    if(state == HIGH){
        setPixelBuffer(index, colourMap[index]);
    }

    if(state == LOW){
        setPixelBuffer(index, Color(0,0,0));
        pressedButton = btnName;
    }
}

void SimonSaysModule::set(std::vector<std::string> args) {

    if(args.size() == 1){
        int newState = stoi(args[0]);

        if(newState == 0){
            reset();
        }else if (newState == 2){
            complete();
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
        threadID = -1;
    }

    state = 0;
    currentRound = 1;

    threadID = threads.addThread(loopTrampoline, this);
}

void SimonSaysModule::complete(){

    setPixelBuffer({strip.Color(0, 255, 0), strip.Color(0, 255, 0), strip.Color(0, 255, 0), strip.Color(0, 255, 0)});

    if(threadID != -1){
        threads.kill(threadID);
        threadID = -1;
    }

    state = 0;
    currentRound = 1;
    state = 2;

}

int SimonSaysModule::getInputSequence(){

    int startTime = millis(); 

    std::vector<int> inputSequence = std::vector<int>();

    while(millis() - startTime < 4000){

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

                if(inputSequence.size() > currentRound) return 0;

                for(size_t i = 0; i < inputSequence.size(); i++){
                    if(inputSequence[i] != correctSequence[i]) return 0;
                }

                if(inputSequence.size() == currentRound){
                    startTime -= 3000;
                }
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

void SimonSaysModule::setPixelBuffer(std::vector<uint32_t> newColours){

    pixelBufferMutex.lock();
    pixelBuffer = newColours;
    pixelBufferMutex.unlock();
    
}

void SimonSaysModule::setPixelBuffer(int index, uint32_t newColour){

    pixelBufferMutex.lock();
    pixelBuffer[index] = newColour;
    pixelBufferMutex.unlock();

}

void SimonSaysModule::loop(){

    while(true){

        if((currentRound > 1) & (state != 1)) state = 1;

        for(size_t i = 0; i < currentRound; i++){

            setPixelBuffer(correctSequence[i], colourMap[correctSequence[i]]);

            uint32_t waitUntil = millis() + 500;
            while (millis() < waitUntil) threads.yield();

            setPixelBuffer({strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0)});

            if( i < currentRound - 1){
                waitUntil = millis() + 200;
                while (millis() < waitUntil) threads.yield();
            }
        }

        int result = getInputSequence();

        if(result == 2){
            if(currentRound < correctSequence.size()){
                currentRound++;
            }else{
                complete();
                return;
            }
        }else if(result == 0){
        
            setPixelBuffer({strip.Color(255, 0, 0), strip.Color(255, 0, 0), strip.Color(255, 0, 0), strip.Color(255, 0, 0)});

            uint32_t waitUntil = millis() + 100;
            while (millis() < waitUntil) threads.yield();

            setPixelBuffer({strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0)});   
            
            waitUntil = millis() + 100;
            while (millis() < waitUntil) threads.yield();

            setPixelBuffer({strip.Color(255, 0, 0), strip.Color(255, 0, 0), strip.Color(255, 0, 0), strip.Color(255, 0, 0)}); 

            waitUntil = millis() + 100;
            while (millis() < waitUntil) threads.yield();

            setPixelBuffer({strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0)}); 

            waitUntil = millis() + 100;
            while (millis() < waitUntil) threads.yield();

            setPixelBuffer({strip.Color(255, 0, 0), strip.Color(255, 0, 0), strip.Color(255, 0, 0), strip.Color(255, 0, 0)}); 

            waitUntil = millis() + 100;
            while (millis() < waitUntil) threads.yield();

            setPixelBuffer({strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0), strip.Color(0, 0, 0)}); 

            state = 0;
            currentRound = 1;
        }
    }

}