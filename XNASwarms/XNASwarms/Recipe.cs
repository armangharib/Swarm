﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNASwarms
{
    public class Recipe {

	private List<Parameters> parameters;
	private List<int> popCounts;
	private String recipeText;

	private static double populationChangeMagnitude = 0.8;
	private static double duplicationOrDeletionRatePerParameterSets = 0.1;
	private static double randomAdditionRatePerRecipe = 0.1;
	private static double pointMutationRatePerParameter = 0.1;
	private static double pointMutationMagnitude = 0.5;

    Random rand = new Random();

	public Recipe(String text) {
		setFromText(text);
	}

	public Recipe(List<Individual> sol) {
		setFromPopulation(sol);
	}

	public bool setFromText(String text) {  
		char ch;
		int numberOfIngredients, numberOfIndividuals;
		double neighborhoodRadius, normalSpeed, maxSpeed, c1, c2, c3, c4, c5;

        StringBuilder recipeProcessed = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            ch = text[i];
            if ((ch >= '0' && ch <= '9') || (ch == '.'))
                recipeProcessed.Append(ch);
            else if (recipeProcessed.Length > 0)
            {
                if (recipeProcessed[recipeProcessed.Length - 1] != ' ')
                    recipeProcessed.Append(' ');
            }
        }

        string[] st = recipeProcessed.ToString().Split(' ');


		if (st.Length % 9 != 0) {
			recipeText = "*** Formatting error ***\n" + text;
			parameters = null;
			popCounts = null;
			return false;
		}

		numberOfIngredients = st.Length / 9;
		if (numberOfIngredients == 0) {
			recipeText = "*** No ingredients ***\n" + text;
			parameters = null;
			popCounts = null;
			return false;
		}
		if (numberOfIngredients > Parameters.numberOfIndividualsMax)
			numberOfIngredients = Parameters.numberOfIndividualsMax;

		parameters = new List<Parameters>();
		popCounts = new List<int>();

		try {
			for (int i = 0; i < numberOfIngredients; i++) 
            {
                numberOfIndividuals = int.Parse(st[0 + i * 9]);
				if (numberOfIndividuals < 1)
					numberOfIndividuals = 1;
                neighborhoodRadius = Double.Parse(st[1 + i * 9]);
                normalSpeed = Double.Parse(st[2 + i * 9]);
                maxSpeed = Double.Parse(st[3 + i * 9]);
                c1 = Double.Parse(st[4 + i * 9]);
                c2 = Double.Parse(st[5 + i * 9]);
                c3 = Double.Parse(st[6 + i * 9]);
                c4 = Double.Parse(st[7 + i * 9]);
                c5 = Double.Parse(st[8 + i * 9]);
				parameters.Add(new Parameters(neighborhoodRadius,
						normalSpeed, maxSpeed, c1, c2, c3, c4, c5));
				popCounts.Add(numberOfIndividuals);
			}
		} catch (Exception nfe) 
        {
            //TODO : Find a better Exception for this catch
            recipeText = "*** Formatting error ***\n" + text;
            parameters = null;
            popCounts = null;
			return false;
		}

		boundPopulationSize();
		setFromPopulation(createPopulation(300, 300));
		return true;
	}

	public void boundPopulationSize() {
		int totalPopulation = 0;
		double rescalingRatio;

		int numberOfIngredients = parameters.Count;

		for (int i = 0; i < numberOfIngredients; i++)
			totalPopulation += popCounts[i];

		if (totalPopulation > Parameters.numberOfIndividualsMax)
			rescalingRatio = (double) (Parameters.numberOfIndividualsMax - numberOfIngredients)
					/ (totalPopulation == numberOfIngredients ? 1.0
							: (double) (totalPopulation - numberOfIngredients));

		else
			rescalingRatio = 1;

		if (rescalingRatio != 1)
			for (int i = 0; i < numberOfIngredients; i++)
				popCounts[i] = (1 + (int)Math.Floor((double)(popCounts[i] - 1) * rescalingRatio));
	}

	public void setFromPopulation(List<Individual> sol) {
		parameters = new List<Parameters>();
		popCounts = new List<int>();

		Parameters tempParam;

		for (int i = 0; i < sol.Count(); i++) {
			tempParam = sol[i].getGenome();

			bool alreadyInParameters = false;
			for (int j = 0; j < parameters.Count; j++) {
				if (parameters[j].equals(tempParam)) {
					alreadyInParameters = true;
					popCounts[j] = (popCounts[j] + 1);
				}
			}
			if (alreadyInParameters == false) {
				parameters.Add(tempParam);
				popCounts.Add(1);
			}
		}

		setRecipeText();
	}

	private void setRecipeText() {
		Parameters tempParam;

		recipeText = "";

		for (int i = 0; i < parameters.Count(); i++) {
			tempParam = parameters[i];
			recipeText += "" + popCounts[i] + " * ("
					+ shorten(tempParam.getNeighborhoodRadius()) + ", "
					+ shorten(tempParam.getNormalSpeed()) + ", "
					+ shorten(tempParam.getMaxSpeed()) + ", "
					+ shorten(tempParam.getC1()) + ", " + shorten(tempParam.getC2())
					+ ", " + shorten(tempParam.getC3()) + ", "
					+ shorten(tempParam.getC4()) + ", " + shorten(tempParam.getC5())
					+ ")\n";
		}
	}

	private double shorten(double d) {
		return Math.Round(d * 100.0) / 100.0;
	}

	public List<Individual> createPopulation(int width, int height) {
		if (parameters == null)
			return null;

		List<Individual> newPopulation = new List<Individual>();
		Parameters tempParam;

		for (int i = 0; i < parameters.Count; i++) {
			tempParam = parameters[i];
			for (int j = 0; j < popCounts[i]; j++)
				newPopulation.Add(new Individual(rand.NextDouble() * width,
                        rand.NextDouble() * height, rand.NextDouble() * 10 - 5,
                        rand.NextDouble() * 10 - 5, new Parameters(
								tempParam)));
		}
        mutate();
		return newPopulation;
	}
	 
	public bool mutate() {
		bool mutated = false;
		int numberOfIngredients = parameters.Count();

		// Insertions, duplications and deletions

		for (int j = 0; j < numberOfIngredients-1; j++) {
            if (rand.NextDouble() < duplicationOrDeletionRatePerParameterSets)
            {
				if (rand.NextDouble() < .5) { // Duplication
					mutated = true;
					parameters[j + 1] = parameters[j];
					popCounts[j + 1] = popCounts[j];
					numberOfIngredients++;
					j++;
				} else { // Deletion
					if (numberOfIngredients > 1) {
						mutated = true;
						parameters.RemoveAt(j);
                        popCounts.RemoveAt(j);
						numberOfIngredients--;
						j--;
					}
				}
			}
		}

        if (rand.NextDouble() < randomAdditionRatePerRecipe)
        { // Addition
			mutated = true;
			parameters.Add(new Parameters());
            popCounts.Add((int)(rand.NextDouble() * Parameters.numberOfIndividualsMax * 0.5) + 1);
		}

		// Then Point Mutations

		Parameters tempParam;

		for (int j = 0; j < numberOfIngredients; j++) {
			tempParam = new Parameters(parameters[j]);
			tempParam.inducePointMutations(pointMutationRatePerParameter,
					pointMutationMagnitude);
			if (!parameters[j].equals(tempParam)) {
				mutated = true;
				parameters[j] = tempParam;
			}
		}

		boundPopulationSize();
		
		return mutated;
	}
	
	public String getRecipeText() {
		setRecipeText();
		return recipeText;
	}
  }
}