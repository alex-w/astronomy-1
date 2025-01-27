//[astronomy](../../../index.md)/[io.github.cosinekitty.astronomy](../index.md)/[ApsisInfo](index.md)

# ApsisInfo

class [ApsisInfo](index.md)(time: [Time](../-time/index.md), kind: [ApsisKind](../-apsis-kind/index.md), distanceAu: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html))

An apsis event: pericenter (closest approach) or apocenter (farthest distance).

For the Moon orbiting the Earth, or a planet orbiting the Sun, an *apsis* is an event where the orbiting body reaches its closest or farthest point from the primary body. The closest approach is called *pericenter* and the farthest point is *apocenter*.

More specific terminology is common for particular orbiting bodies. The Moon's closest approach to the Earth is called *perigee* and its farthest point is called *apogee*. The closest approach of a planet to the Sun is called *perihelion* and the furthest point is called *aphelion*.

This data structure is returned by [searchLunarApsis](../search-lunar-apsis.md) and [nextLunarApsis](../next-lunar-apsis.md) to iterate through consecutive alternating perigees and apogees.

## Constructors

| | |
|---|---|
| [ApsisInfo](-apsis-info.md)<br>fun [ApsisInfo](-apsis-info.md)(time: [Time](../-time/index.md), kind: [ApsisKind](../-apsis-kind/index.md), distanceAu: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)) |

## Properties

| Name | Summary |
|---|---|
| [distanceAu](distance-au.md)<br>val [distanceAu](distance-au.md): [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)<br>The distance between the centers of the bodies in astronomical units. |
| [distanceKm](distance-km.md)<br>val [distanceKm](distance-km.md): [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)<br>The distance between the centers of the bodies in kilometers. |
| [kind](kind.md)<br>val [kind](kind.md): [ApsisKind](../-apsis-kind/index.md)<br>Whether this is a pericenter or apocenter event. |
| [time](time.md)<br>val [time](time.md): [Time](../-time/index.md)<br>The date and time of the apsis. |
