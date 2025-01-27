//[astronomy](../../../index.md)/[io.github.cosinekitty.astronomy](../index.md)/[Observer](index.md)

# Observer

data class [Observer](index.md)(latitude: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), longitude: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), height: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html))

The location of an observer on (or near) the surface of the Earth.

This object is passed to functions that calculate phenomena as observed from a particular place on the Earth.

## Constructors

| | |
|---|---|
| [Observer](-observer.md)<br>fun [Observer](-observer.md)(latitude: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), longitude: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), height: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)) |

## Functions

| Name | Summary |
|---|---|
| [toStateVector](to-state-vector.md)<br>fun [toStateVector](to-state-vector.md)(time: [Time](../-time/index.md), equator: [EquatorEpoch](../-equator-epoch/index.md)): [StateVector](../-state-vector/index.md)<br>Calculates geocentric equatorial position and velocity of an observer on the surface of the Earth. |
| [toVector](to-vector.md)<br>fun [toVector](to-vector.md)(time: [Time](../-time/index.md), equator: [EquatorEpoch](../-equator-epoch/index.md)): [Vector](../-vector/index.md)<br>Calculates geocentric equatorial coordinates of an observer on the surface of the Earth. |

## Properties

| Name | Summary |
|---|---|
| [height](height.md)<br>val [height](height.md): [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)<br>The height above (positive) or below (negative) sea level, expressed in meters. |
| [latitude](latitude.md)<br>val [latitude](latitude.md): [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)<br>Geographic latitude in degrees north (positive) or south (negative) of the equator. |
| [longitude](longitude.md)<br>val [longitude](longitude.md): [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)<br>Geographic longitude in degrees east (positive) or west (negative) of the prime meridian at Greenwich, England. |
