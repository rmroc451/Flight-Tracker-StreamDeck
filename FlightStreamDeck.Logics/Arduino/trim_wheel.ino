﻿/*

*/

void setup() {
 Serial.begin(9600);
}

int priorSensorValue = 0;
int pMin = 0;  //the lowest value that comes out of the potentiometer
int pMax = 1023; //the highest value that comes out of the potentiometer.

int convMin = -16383;  //the lowest value you want set in the simulator.
int convMax = 16383; //the highest value you want set in the simulator.

void loop() {
 int sensorValue = analogRead(A0);
 if (sensorValue != priorSensorValue && abs(priorSensorValue - sensorValue) > 3) {
   priorSensorValue = sensorValue;  
   Serial.println(map(sensorValue, pMin, pMax, convMin, convMax));
 }
 delay(10);
}
