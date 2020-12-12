/*

*/

#define RED_LED   13
#define GRN_LED   12
//const int firstRotaryPin = 7;
//const int lastRotaryPin = 11;

void setup() {
 Serial.begin(9600);
 pinMode(RED_LED, OUTPUT);
 pinMode(GRN_LED, OUTPUT);
}

int priorSensorValueA0 = -1;
int priorSensorValueA1 = -1;
bool priorSensorValueA2 = -1;
bool priorSensorValueA3 = -1;
bool priorSensorValueA4 = -1;
bool priorSensorValueA5 = -1;
bool priorSensorValueA6 = -1;
bool priorSensorValueA7 = -1;
bool priorSensorValueA8 = -1;
bool priorSensorValueA9 = -1;
bool priorSensorValueA10 = -1;
bool priorSensorValueA11 = -1;
bool priorSensorValueA12 = -1;
int pMin = 0;  //the lowest value that comes out of the potentiometer
int pMax = 1023; //the highest value that comes out of the potentiometer.


void loop() {
  trimLoop(analogRead(A0));
  starterSwitchLoop(analogRead(A1));
  masterAnalogLoop(analogRead(A2), "A2:MASTER_ALTERNATOR_TOGGLE", priorSensorValueA2, true);
  masterAnalogLoop(analogRead(A3), "A3:MASTER_BATTERY_TOGGLE", priorSensorValueA3, true);
  masterAnalogLoop(analogRead(A4), "A4:FUEL_PUMP", priorSensorValueA4, true);
  masterAnalogLoop(analogRead(A5), "A5:TOGGLE_BEACON_LIGHTS", priorSensorValueA5, true);
  masterAnalogLoop(analogRead(A6), "A6:LANDING_LIGHTS_TOGGLE", priorSensorValueA6, true);
  masterAnalogLoop(analogRead(A7), "A7:TOGGLE_TAXI_LIGHTS", priorSensorValueA7, true);
  masterAnalogLoop(analogRead(A8), "A8:TOGGLE_NAV_LIGHTS", priorSensorValueA8, true);
  masterAnalogLoop(analogRead(A9), "A9:STROBES_TOGGLE", priorSensorValueA9, true);
  masterAnalogLoop(analogRead(A10), "A10:PITOT_HEAT_TOGGLE", priorSensorValueA10, true);
  masterAnalogLoop(analogRead(A11), "A11", priorSensorValueA11, priorSensorValueA12);
  masterAnalogLoop(analogRead(A12), "A12", priorSensorValueA12, priorSensorValueA11);
 
 delay(10);
}

void trimLoop(int sensorValue) {
  if (sensorValue != priorSensorValueA0 && abs(priorSensorValueA0 - sensorValue) > 3) {
   priorSensorValueA0 = sensorValue;
   Serial.print("A0:");
   Serial.println(map(sensorValue, pMin, pMax, -16383, 16383));
 }
}

void starterSwitchLoop(int sensorValue) {
  if (sensorValue != priorSensorValueA1 && (abs(priorSensorValueA1 - sensorValue) >= 25 || sensorValue == 0)) {
   priorSensorValueA1 = sensorValue;
   if (sensorValue >= 1000) {
     Serial.println("A1:0");
   } else if (sensorValue >= 700) {
     Serial.println("A1:1");
   } else if (sensorValue >= 400) {
     Serial.println("A1:2");
   } else if (sensorValue >= 100) {
     Serial.println("A1:3");
   } else if (sensorValue >= 0) {
     Serial.println("A1:4");
   }
 }
}

void masterAnalogLoop(int sensorValue, String pin, bool& priorSensorValue, bool alsoRequired){
  bool boolSensorValue = (sensorValue > 50);
  if (boolSensorValue != priorSensorValue) {
   priorSensorValue = boolSensorValue;
   Serial.println(pin + ":" + String(boolSensorValue && alsoRequired));
  }
}

void colorLoop() {
  // red on
  digitalWrite(RED_LED, HIGH);
  digitalWrite(GRN_LED, LOW);
  delay(10);
  // green on
  digitalWrite(RED_LED, LOW);
  digitalWrite(GRN_LED, HIGH);
  delay(10);
  // both on
  digitalWrite(RED_LED, HIGH);
  delay(10);
}
