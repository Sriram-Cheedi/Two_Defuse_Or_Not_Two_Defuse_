
char receivedChar;
boolean newData = false;

boolean lastBtnState = HIGH;

const int btn = 50;

void setup() {
    Serial.begin(115200);

    pinMode(btn, INPUT_PULLUP);

}

void loop() {
    
    receiveSerial();

    if(digitalRead(btn) == HIGH && lastBtnState == LOW){
        Serial.print("$BTN,1#");
        lastBtnState = HIGH;

    }else if (digitalRead(btn) == LOW && lastBtnState == HIGH){
        Serial.print("$BTN,0#");
        lastBtnState = LOW;
    }
}


std::string receiveSerial() {
    
    while(Serial.available()){

        if(Serial.read() != '$') break;

        String cmd = Serial.readStringUntil('#');

        if(cmd != ""){
            return cmd;
        }
    }
}
