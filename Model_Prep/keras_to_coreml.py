"""A script to convert a Keras Model to a CoreML model and save it"""

# General imports
import os
import argparse

# ML imports
import coremltools

# Imports from our code
import models

INPUT_NAME_DEFAULT = 'input1'
OUTPUT_NAME_DEFAULT = 'output1'

def convert_and_save_model(model, output_model_file, input_name = INPUT_NAME_DEFAULT, output_name = OUTPUT_NAME_DEFAULT):
    """Converts a Keras model to a CoreML model and saves the CoreML model to `output_model_file`

    Args:
        model: keras.models.Model
            the model to convert
        output_model_file: str
            path for where to save the converted model
        input_name: str
            name for the input to the CoreML model
        output_name: str
            name for the output of the CoreML model
    """

    # Convert to CoreML
    print('---- Converting to CoreML...\n')
    coreml_model = coremltools.converters.keras.convert(model, input_names=input_name, output_names=output_name)
    print('\nDone converting to CoreML\n')

    # Save the model
    print('---- Saving CoreML model (to ' + output_model_file + ')...')
    coreml_model.save(output_model_file)
    print('\nDone saving CoreML model\n')
    
    print('-------------------------------------------------------------------------')
    print('Model exported to: ' + output_model_file)
    print('')
    print('Input Name: ' + input_name)
    print('Output Name: ' + output_name)
    print('-------------------------------------------------------------------------')

def main(model_arch, input_model_file, output_model_file, input_name = INPUT_NAME_DEFAULT, output_name = OUTPUT_NAME_DEFAULT):
    # Load model
    print('\n---- Loading Keras model (' + input_model_file + ')...\n')
    model = models.load(model_arch, input_model_file)
    print('\nDone loading model\n')

    convert_and_save_model(model, output_model_file, input_name, output_name)

if __name__ == '__main__':
    # Extract arguments
    parser = argparse.ArgumentParser(description='Convert a Keras model to a CoreML model (.mlmodel)')

    model_architectures = models.available_architectures()
    model_architecture_default = model_architectures[0]
    parser.add_argument('--model_arch', type=str, action='store', choices=model_architectures, default=model_architecture_default, help='the architecture of the model being converted, defaults to ' + model_architecture_default)
    parser.add_argument('--input_model_file', type=str, action='store', help='path to the saved Keras weights file')
    parser.add_argument('--output_model_file', type=str, action='store', required=True, help='path to save the generated mlmodel file')

    parser.add_argument('--input_name', type=str, action='store', default=INPUT_NAME_DEFAULT, help='name to use for the model input, defaults to "' + INPUT_NAME_DEFAULT + '"')
    parser.add_argument('--output_name', type=str, action='store', default=OUTPUT_NAME_DEFAULT, help='name to use for the model output, defaults to "' + OUTPUT_NAME_DEFAULT + '"')

    args = parser.parse_args()

    # Call main function
    main(args.model_arch, args.input_model_file, args.output_model_file, args.input_name, args.output_name)