#include "SimonSaysModule.h"


SimonSaysModule::SimonSaysModule(std::string name, std::vector<int> buttonPins, int rgbPin, SerialController serialController)
: Component(name), buttonPins(buttonPins), rgbPin(rgbPin), serialController(serialController) {

    for(size_t i = 0; i < buttonPins.size(); i++){
        Button btn = Button("SSM" + std::to_string(i), buttonPins[i], serialController, [this](std::string name, int state){buttonPressed(name, state);});   
        buttons.push_back(btn);
    }

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

    int newState = state.load(std::memory_order_acquire);

    if(newState != oldState){
        oldState = newState;
        status();
    }

}

void SimonSaysModule::buttonPressed(std::string btnName, int btnState){

    if((currentRound == 1) && (state.load(std::memory_order_acquire) != 1)) {
        state.store(1, std::memory_order_release);
    }

    int index = stoi(btnName.substr(3));

    if(btnState == HIGH){
        setPixelBuffer(index, colourMap[index]);
    }

    if(btnState == LOW){
        setPixelBuffer(index, Color(0,0,0));
        pressedButton.store(index, std::memory_order_release);
    }
}

void SimonSaysModule::set(std::vector<std::string> args) {

    resetingMutex.lock();

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

    resetingMutex.unlock();
}

void SimonSaysModule::status() {
    serialController.serialWrite(name, std::to_string(state.load(std::memory_order_acquire)));
}

void SimonSaysModule::reset(){



    if(threadID != -1){
        threads.kill(threadID);
        threadID = -1;
    }

    state.store(0, std::memory_order_release);
    currentRound = 1;

    threadID = threads.addThread(loopTrampoline, this);
}

void SimonSaysModule::complete(){

    setPixelBuffer({strip.Color(0, 255, 0), strip.Color(0, 255, 0), strip.Color(0, 255, 0), strip.Color(0, 255, 0)});

    if(threadID != -1){
        threads.kill(threadID);
        threadID = -1;
    }

    currentRound = 1;
    state.store(2, std::memory_order_release);
}

int SimonSaysModule::getInputSequence(){

    int startTime = millis(); 

    std::vector<int> inputSequence = std::vector<int>();

    while(millis() - startTime < 4000){

        for(auto& btn : buttons){
            
            btn.update();

            if(pressedButton.load(std::memory_order_acquire) == -1){
                continue;
            }else{

                startTime = millis();

                if(pressedButton.load(std::memory_order_acquire) == 0){
                    inputSequence.push_back(0);
                }else
                if(pressedButton.load(std::memory_order_acquire) == 1){
                    inputSequence.push_back(1);
                }else 
                if(pressedButton.load(std::memory_order_acquire) == 2){
                    inputSequence.push_back(2);
                }else 
                if(pressedButton.load(std::memory_order_acquire) == 3){
                    inputSequence.push_back(3);
                }

                pressedButton.store(-1, std::memory_order_release);

                if(inputSequence.size() > currentRound) return 0;

                resetingMutex.lock();
                for(size_t i = 0; i < inputSequence.size(); i++){
                    if(inputSequence[i] != correctSequence[i]) {
                        resetingMutex.unlock();
                        return 0;
                    }
                }
                resetingMutex.unlock();

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

    resetingMutex.lock();
    for(size_t i = 0; i < currentRound; i++){
        if(inputSequence[i] != correctSequence[i]) {
            resetingMutex.unlock();
            return 0;
        }
    }
    resetingMutex.unlock();
    
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

void SimonSaysModule::completeSequence(){

    setPixelBuffer({strip.Color(0, 50, 0), strip.Color(0, 50, 0), strip.Color(0, 50, 0), strip.Color(0, 50, 0)});

    while(true){

        uint32_t waitUntil = millis() + 3000;
        while (millis() < waitUntil) threads.yield();

        for(size_t i = 0; i < correctSequence.size(); i++){

            resetingMutex.lock();

            setPixelBuffer(correctSequence[i], colourMap[correctSequence[i]]);

            resetingMutex.unlock();

            uint32_t waitUntil = millis() + 500;
            while (millis() < waitUntil) threads.yield();

            setPixelBuffer({strip.Color(0, 50, 0), strip.Color(0, 50, 0), strip.Color(0, 50, 0), strip.Color(0, 50, 0)});

        }

        
    }
}

void SimonSaysModule::loop(){

    while(true){

        for(size_t i = 0; i < currentRound; i++){

            resetingMutex.lock();

            setPixelBuffer(correctSequence[i], colourMap[correctSequence[i]]);

            resetingMutex.unlock();

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
            resetingMutex.lock();
            if(currentRound < correctSequence.size()){
                currentRound++;
            }else{

                currentRound = 1;
                state.store(2, std::memory_order_release);
                
                resetingMutex.unlock();

                completeSequence();
            }
            resetingMutex.unlock();
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

            waitUntil = millis() + 500;
            while (millis() < waitUntil) threads.yield();

            state.store(0, std::memory_order_release);
            currentRound = 1;
        }
    }

}