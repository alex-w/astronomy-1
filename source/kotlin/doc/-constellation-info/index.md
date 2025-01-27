//[astronomy](../../../index.md)/[io.github.cosinekitty.astronomy](../index.md)/[ConstellationInfo](index.md)

# ConstellationInfo

class [ConstellationInfo](index.md)(symbol: [String](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-string/index.html), name: [String](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-string/index.html), ra1875: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), dec1875: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html))

Reports the constellation that a given celestial point lies within.

The [constellation](../constellation.md) function returns this object to report which constellation corresponds with a given point in the sky. Constellations are defined with respect to the B1875 equatorial system per IAU standard. Although constellation requires J2000 equatorial coordinates, ConstellationInfo contains converted B1875 coordinates for reference.

## Constructors

| | |
|---|---|
| [ConstellationInfo](-constellation-info.md)<br>fun [ConstellationInfo](-constellation-info.md)(symbol: [String](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-string/index.html), name: [String](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-string/index.html), ra1875: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), dec1875: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)) |

## Properties

| Name | Summary |
|---|---|
| [dec1875](dec1875.md)<br>val [dec1875](dec1875.md): [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)<br>Declination expressed in B1875 coordinates. |
| [name](name.md)<br>val [name](name.md): [String](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-string/index.html)<br>Full name of constellation, e.g. "Orion". |
| [ra1875](ra1875.md)<br>val [ra1875](ra1875.md): [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)<br>Right ascension expressed in B1875 coordinates. |
| [symbol](symbol.md)<br>val [symbol](symbol.md): [String](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-string/index.html)<br>3-character mnemonic symbol for the constellation, e.g. "Ori". |
