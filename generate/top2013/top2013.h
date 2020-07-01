/*
    top2013.h  -  Don Cross  -  2020-06-30

    Loads and calculates planetary positions using TOP2013 analytic models.
    See:  https://en.wikipedia.org/wiki/VSOP_(planets)
*/
#ifndef __DDC_TOP2013_H
#define __DDC_TOP2013_H

typedef struct
{
    double  k;      /* mu coefficient */
    double  c;      /* cosine coefficient */
    double  s;      /* sine coefficient */
    double  p;      /* period expressed in years */
    int     rc;     /* hack: final digit rounding adjustment for 'c' */
    int     rs;     /* hack: final digit rounding adjustment for 's' */
}
top_term_t;


typedef struct
{
    int nterms_total;
    int nterms_calc;
    top_term_t *terms;
}
top_series_t;

#define TOP_MAX_SERIES 13

typedef struct
{
    int nseries_total;
    int nseries_calc;
    top_series_t series[TOP_MAX_SERIES];
}
top_formula_t;


#define TOP_NCOORDS 6

typedef struct
{
    int planet;     /* 5=Jupiter, 6=Saturn, 7=Uranus, 8=Neptune, 9=Pluto */
    top_formula_t formula[TOP_NCOORDS];
}
top_model_t;


typedef struct
{
    double a;           /* AU */
    double lambda;      /* rad */
    double k;           /* 1 */
    double h;           /* 1 */
    double q;           /* 1 */
    double p;           /* 1 */
}
top_elliptical_t;


typedef struct
{
    /* Position in AU */
    double  x;
    double  y;
    double  z;

    /* velocity in AU/day */
    double  vx;
    double  vy;
    double  vz;
}
top_rectangular_t;


void TopInitModel(top_model_t *model);
void TopFreeModel(top_model_t *model);
int  TopLoadModel(top_model_t *model, const char *filename, int planet);
int  TopSaveModel(const top_model_t *model, const char *filename);
int  TopWriteModel(const top_model_t *model, FILE *outfile);
int  TopCalcElliptical(const top_model_t *model, double tt, top_elliptical_t *ellip);
int  TopEcliptic(int planet, const top_elliptical_t *ellip, top_rectangular_t *ecl);
int  TopEquatorial(const top_rectangular_t *ecl, top_rectangular_t *equ);

#endif /* __DDC_TOP2013_H */