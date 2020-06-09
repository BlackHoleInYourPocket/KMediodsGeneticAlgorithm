using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace KMediodsGeneticAlgorithm
{
    public partial class FileSelection : Form
    {
        public static List<List<double>> all_data;
        public static Population population;
        int p = 0;
        string file = "";
        int k = 0;
        int iterateNumber = 0;
        public FileSelection()
        {
            InitializeComponent();
        }

        private void FileSelection_Load(object sender, EventArgs e)
        {

        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtPath.Text = openFileDialog1.FileName;
            }
        }
        private void TxtPath_TextChanged(object sender, EventArgs e)
        {
            file = txtPath.Text;
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            resultBox.Text = "";
            p = Convert.ToInt32(populationSize.Text);
            k = Convert.ToInt32(kNumber.Text);
            iterateNumber = Convert.ToInt32(iterationNumber.Text);

            population = new Population(p, k);
            all_data = new List<List<double>>();

            StreamReader sr = new StreamReader(file);
            string line = "";
            do
            {
                line = sr.ReadLine();
            } while (line != "@data" && line != "@DATA");

            do
            {
                line = sr.ReadLine();

                if (line.Length < 3)
                    break;

                string[] features = line.Split(',');
                all_data.Add(new List<double>());

                for (int i = 0; i < features.Length - 1; i++)
                {
                    all_data[all_data.Count - 1].Add(Convert.ToDouble(features[i].Replace(".", ",")));
                }

            } while (!sr.EndOfStream);

            initialization(k);

            for (int i = 0; i < p; i++)
            {
                population.data[i].fitness_value = calculateFF(population.data[i]);
            }


            #region Crossovers - Mutation
            Random rnd = new Random();
            int mutationNumber = 0;
            int mutationPercent = 30;
            for (int crossoverNumber = 0; crossoverNumber < iterateNumber; crossoverNumber++)
            {
                int crossIndex;
                double worstChromosomeFitness = double.MinValue;
                double bestchrosomeFitness = double.MaxValue;
                double secondBestChromosomeFitness = double.MaxValue;
                double newChromosomeBestFitness = double.MaxValue;
                int bestChromosomeIndex = 0;
                int secondBestChromosomeIndex = 0;
                int newBestChromosomeIndex = 0;
                int minFFIndex = 0;
                List<Chromosome> newChromosomes = new List<Chromosome>();
                Chromosome newChromosome = new Chromosome(k);
                Random rndMutation = new Random();
                for (int i = 0; i < p; i++)
                {
                    if (population.data[i].fitness_value < bestchrosomeFitness)
                    {
                        bestchrosomeFitness = population.data[i].fitness_value;
                        bestChromosomeIndex = i;
                    }
                }

                for (int mutant = 0; mutant < population.data.Length; mutant++)
                {
                    int mutationChance = rndMutation.Next(100);
                    //Mutation percent = 0.1;
                    if (mutationChance < mutationPercent && mutant != bestChromosomeIndex)
                    {
                        #region Mutation
                        mutationNumber++;
                        Random rndCentroid = new Random();
                        int mutantK = rndCentroid.Next(1, k);
                        List<int> selected_index = new List<int>();
                        int index = 0;
                        do
                        {
                            index = rnd.Next(0, all_data.Count);
                        } while (selected_index.Count != 0 && selected_index.Contains(index));

                        selected_index.Add(index);
                        population.data[mutant].centroid[mutantK].lst = new List<double>();
                        for (int i = 0; i < all_data[0].Count; i++)
                        {
                            population.data[mutant].centroid[mutantK].lst.Add(all_data[index][i]);
                        }
                        population.data[mutant].fitness_value = calculateFF(population.data[mutant]);

                        #endregion
                    }
                }

                for (int i = 0; i < p; i++)
                {
                    if ((population.data[i].fitness_value < secondBestChromosomeFitness) && i != bestChromosomeIndex)
                    {
                        secondBestChromosomeFitness = population.data[i].fitness_value;
                        secondBestChromosomeIndex = i;
                    }
                }

                newChromosome = new Chromosome(k);
                crossIndex = rnd.Next(1, k);
                for (int j = 0; j < crossIndex; j++)
                {
                    newChromosome.centroid[j] = population.data[bestChromosomeIndex].centroid[j];
                }

                for (int j = crossIndex; j < k; j++)
                {
                    newChromosome.centroid[j] = population.data[secondBestChromosomeIndex].centroid[j];
                }
                newChromosome.fitness_value = calculateFF(newChromosome);
                newChromosomes.Add(newChromosome);

                for (int i = 0; i < newChromosomes.Count; i++)
                {
                    if (newChromosomes[i].fitness_value < newChromosomeBestFitness)
                    {
                        newChromosomeBestFitness = newChromosomes[i].fitness_value;
                        newBestChromosomeIndex = i;
                    }
                }

                for (int i = 0; i < p; i++)
                {
                    if (population.data[i].fitness_value > worstChromosomeFitness)
                    {
                        worstChromosomeFitness = population.data[i].fitness_value;
                        minFFIndex = i;
                    }
                }
                population.data[minFFIndex] = newChromosomes[newBestChromosomeIndex];
            }
            #endregion

            double maxFFAfterCrossover = double.MaxValue;
            int maxFFIndexAfterCrossover = 0;
            for (int i = 0; i < p; i++)
            {
                if (population.data[i].fitness_value < maxFFAfterCrossover)
                {
                    maxFFAfterCrossover = population.data[i].fitness_value;
                    maxFFIndexAfterCrossover = i;
                }
            }

            for (int i = 0; i < population.data[maxFFIndexAfterCrossover].centroid.Length; i++)
            {
                resultBox.AppendText(i + 1 + ". Centroid : \t");
                for (int j = 0; j < population.data[maxFFIndexAfterCrossover].centroid[i].lst.Count; j++)
                {
                    resultBox.AppendText(population.data[maxFFIndexAfterCrossover].centroid[i].lst[j] + "\t");
                }
                resultBox.AppendText(System.Environment.NewLine);
            }
            resultBox.AppendText("Fitness Value : " + population.data[maxFFIndexAfterCrossover].fitness_value);
            resultBox.AppendText(System.Environment.NewLine);
            resultBox.AppendText("Mutation Chance: %" + mutationPercent + " and Total Mutation Number : " + mutationNumber);

        }
        public static void initialization(int k)
        {
            Random rnd = new Random();

            for (int p_index = 0; p_index < population.data.Length; p_index++)
            {
                List<int> selected_index = new List<int>();

                for (int k_index = 0; k_index < k; k_index++)
                {
                    int index = 0;

                    do
                    {
                        index = rnd.Next(0, all_data.Count);
                    } while (selected_index.Count != 0 && selected_index.Contains(index));

                    selected_index.Add(index);

                    for (int i = 0; i < all_data[0].Count; i++)
                    {
                        population.data[p_index].centroid[k_index].lst.Add(all_data[index][i]);
                    }
                }
            }
        }
        public static double calculateFF(Chromosome chromosome)
        {
            List<int> clusters = new List<int>();

            for (int i = 0; i < all_data.Count; i++)
            {
                double min_val = Double.MaxValue;
                int min_index = -1;

                for (int j = 0; j < chromosome.centroid.Length; j++)
                {
                    double resultED = calculateED(all_data[i], chromosome.centroid[j].lst);

                    if (min_val > resultED)
                    {
                        min_val = resultED;
                        min_index = j;
                    }
                }

                clusters.Add(min_index);
            }
            return calculateSSE(clusters, chromosome);
        }

        public static double calculateED(List<double> lst1, List<double> lst2)
        {
            double result = 0;

            for (int i = 0; i < lst1.Count; i++)
            {
                result += Math.Pow(lst1[i] - lst2[i], 2);
            }
            return Math.Sqrt(result);
        }


        public static double calculateSSE(List<int> clusters, Chromosome chr)
        {
            double result = 0;

            for (int i = 0; i < clusters.Count; i++)
            {
                result += calculateED(chr.centroid[clusters[i]].lst, all_data[i]);
            }
            return result;
        }

    }

    public class Centroid
    {
        public List<double> lst = new List<double>();
    }
    public class Chromosome
    {
        public Centroid[] centroid;
        public double fitness_value;

        public Chromosome(int k)
        {
            centroid = new Centroid[k];
            for (int i = 0; i < k; i++)
            {
                centroid[i] = new Centroid();
            }
        }
    }

    public class Population
    {

        public Chromosome[] data;


        public Population(int p, int k)
        {
            data = new Chromosome[p];
            for (int i = 0; i < p; i++)
            {
                data[i] = new Chromosome(k);
            }
        }

    }
}
