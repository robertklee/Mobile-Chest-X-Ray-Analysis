"""A script to extract the final layer weights of a trained ChestXRay model to a csv file"""

# General imports
import argparse

# ML imports
import numpy as np

# Imports from our code
import models

def extract_and_save_weights(model, output_weights_file):
    """Extracts the final layer weights from a Keras model and saves them in CSV format to `output_weights_file`

    Args:
        model: keras.models.Model
            the model to extract weights from
        output_weights_file: str
            path for where to save the extracted weights
    """

    # Extract weights
    print('---- Extracting weights...')
    # We want the weights for the connections between the second last layer (global average pooling)
    # and the last layer (the 14-element Dense layer)
    weights = model.layers[-1].get_weights()[0] # The weights are wrapped inside a list, so we just take the first/only element of that list
    print('Done extracting weights\n')

    # Tranpose the weights since the app consumes the weights in the transposed dimensions (14 x 1024 instead of the original 1024 x 14)
    weights = np.transpose(weights)

    # Save the weights to a csv file
    print('---- Exporting weights...')
    np.savetxt(output_weights_file, weights, delimiter=',', fmt="%.24f")
    print('Done exporting weights\n')

    print('-------------------------------------------------------------------------')
    print('Weights exported to: ' + output_weights_file)
    print('-------------------------------------------------------------------------')

def main(input_model_file, output_weights_file):
    # Load the model
    print('\n---- Loading Keras model (' + input_model_file + ')...\n')
    model = models.load("chestxray", input_model_file)
    print('\nDone loading model\n')

    extract_and_save_weights(model, output_weights_file)

if __name__ == '__main__':
    # Extract arguments
    parser = argparse.ArgumentParser(description='Extract the final layer weights of a trained ChestXRay model and save them to a csv file')
    parser.add_argument('input_model_file', type=str, action='store', help='path to the saved Keras ChestXRay model')
    parser.add_argument('output_weights_file', type=str, action='store', help='path to save the extracted weights in a csv format')
    args = parser.parse_args()

    # Call main function
    main(args.input_model_file, args.output_weights_file)